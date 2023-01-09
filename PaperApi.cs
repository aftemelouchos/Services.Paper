using System.Security.Claims;
using AutoMapper;
using CustomResponseManager.Exceptions;
using GenericRepositoryManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Paper.Model;
using Services.Paper.Model.Dto;
using Services.Paper.Model.Request;
using Services.Paper.Repositories;

namespace Services.Paper.Controllers
{
    [Route("api/Paperapi")]
    [ApiController]
    public class PaperApi : ControllerBase
    {
        private readonly ILogger<PaperApi> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly string _apiGateway;

        public PaperApi(ILogger<PaperApi> logger, IUnitOfWork uow, IMapper mapper,IConfiguration configuration)
        {
            _logger = logger;
            _uow = uow;
            _mapper = mapper;
            _apiGateway = configuration.GetValue<string>("ApiGateway:Address");
        }

        [Route("{id:Guid}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _uow.Paper.GetAsync(id);
            
            if (result is null)
                throw new DataNotFoundException(id);

            var data = _mapper.Map<PaperDto>(result);

            return Ok(data);
        }

        [Authorize(Roles = "AcademicUser")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaperCreateRequest data)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var insert = _mapper.Map<Model.Paper>(data);
            insert.Id = Guid.NewGuid();
            insert.UserId = userId;

            await _uow.Paper.InsertAsync(insert);

            _logger.LogInformation("The data [{@Data}] inserted.", new { insert.Id, UserId = userId });
           
            var uri = new Uri($"{_apiGateway}/Paper/{insert.Id}");
            return Created(uri, insert);

        }

        [Authorize(Roles = "AcademicUser")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PaperUpdateRequest data)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var isExist = await _uow.Paper.GetAsync(data.Id);

            if (isExist is null)
                throw new DataNotFoundException(data.Id);

            var update = _mapper.Map<Model.Paper>(data);
            update.Id = data.Id;
            update.UserId = userId;

            if (isExist.UserId != userId)
                throw new UnprocessableException(data.Id);

            await _uow.Paper.UpdateAsync(update);
            _logger.LogInformation("The data [{@Data}] updated.", new { data.Id, UserId = userId });

            return NoContent();
        }

        [Route("{id:Guid}")]
        [Authorize(Roles = "AcademicUser")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var isExist = await _uow.Paper.GetAsync(id);
            if (isExist is null)
                throw new DataNotFoundException(id);

            if (isExist.UserId != userId) throw new UnprocessableException(id);

            await _uow.Paper.DeleteRowAsync(id);
            _logger.LogInformation("The data [{@Data}] deleted.", new { Id = id, UserId = userId });

            return NoContent();
        }

        [Route("getall/{id:Guid}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(Guid id)
        {
            var result = await _uow.Paper.GetAllByUserIdAsync(id);

            if (!result.Any())
                throw new DataNotFoundException(id);

            var data = _mapper.Map<List<PaperDto>>(result);
            
            return Ok(data);
        }

        [Route("getall/{id:Guid}/{p:int}/{s:int}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllWithPaging(Guid id, int p, int s)
        {
            var result = await _uow.Paper.GetAllByUserIdWithPagingAsync(id, p, s);

            if (!((IEnumerable<Model.Paper>) result.Data).Any())
                throw new DataNotFoundException(id);

            result.Data = _mapper.Map<List<PaperDto>>(result.Data);

            return Ok(result);
        }

        [Route("list/{id:Guid}/{p:int}/{s:int}")]
        [HttpGet]
        [Authorize(Roles = "AcademicUser")]
        public async Task<IActionResult> GetAllWithPaging(Guid id, int p, int s, string? search)
        {

            var result = await _uow.Paper.GetAllWithPagingAsync(id, p, s, search);

            if (result.Data is DBNull)
                throw new DataNotFoundException(id);

            return Ok(result);
        }

        [Route("getallfromservice/{id:Guid}/{p:int}/{s:int}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllFromServiceWithPaging(Guid id, int p, int s)
        {
            var tc = _uow.User.GetUserId(id);
            if (tc is null)
                throw new BadRequestException(id);

            var result =
                await _uow.Yoksis.Get<PaperYoksisResult>($"api/AkademikPersonel/Bildiri/{tc.Result.IdentityNo}");

            if (result is null)
                throw new DataNotFoundException(id);

            var data = result.AsQueryable().ToPager(p,s);

            data.Data = _mapper.Map<IEnumerable<PaperDto>>(data.Data);

            return Ok(data);
        }
    }
}
