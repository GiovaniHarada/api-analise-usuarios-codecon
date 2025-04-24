using AnlUsuarui.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders.Composite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AnlUsuarui.Controllers
{
    [ApiController]
    [Route("api")]
    public class UsuariosController : ControllerBase
    {


        private readonly ILogger<UsuariosController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly string _uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        private readonly string _cacheKey = "USUARIOS";

        public UsuariosController(ILogger<UsuariosController> logger,IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            if (!Directory.Exists(_uploads))
            {
                Directory.CreateDirectory(_uploads);
            }
        }

        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> AddUsers(IFormFile file) 
        {
            

            if (file.Length > 0)
            {
                string filePath = Path.Combine(_uploads, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                string readContents;
                using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
                {
                    readContents = streamReader.ReadToEnd();
                }
                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                var obj = JsonSerializer.Deserialize<UsuarioModel[]>(readContents, serializeOptions);
                _memoryCache.Set(_cacheKey, obj);


                var response = new InsertResponse { Message = "Arquivo recebido com sucesso", UserCount = obj.Length };
                return Ok(response);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("superusers")]
        public async Task<IActionResult> GetSuperUsers()
        {
            var time1 = Stopwatch.GetTimestamp();
            var response = new SuperUsersResponse();
            if(_memoryCache.TryGetValue(_cacheKey, out UsuarioModel[]? usuarios))
            {
                if(usuarios == null || usuarios.Length == 0)
                {
                    return BadRequest("Enviar usuarios antes");
                }
                response.Data = usuarios.Where(u => u.Score > 900 && u.Ativo == true).ToList();
                response.Count = response.Data.Count();
                response.ExecutionTimeMs = Stopwatch.GetElapsedTime(time1).TotalMilliseconds;
                response.Timestamp = Stopwatch.GetTimestamp();

                return Ok(response);
            }

            return BadRequest("Enviar usuarios antes");
        }

        [HttpGet]
        [Route("top-countries")]
        public async Task<IActionResult> GetTopCountries()
        {
            var time1 = Stopwatch.GetTimestamp();
            var response = new TopCountriesResponse();
            if (!_memoryCache.TryGetValue(_cacheKey, out UsuarioModel[]? usuarios))
            {
                return BadRequest("Enviar usuarios antes");
            }

            if (usuarios == null || usuarios.Length == 0)
            {
                return BadRequest("Enviar usuarios antes");
            }

            var dict = new Dictionary<string, List<UsuarioModel>>();
            var listCount = new List<KeyValuePair<string, int>>();
            foreach (var usuario in usuarios)
            {
                if (!dict.ContainsKey(usuario.Pais))
                {
                    dict[usuario.Pais] = new List<UsuarioModel>();
                    listCount.Add(new KeyValuePair<string, int>(usuario.Pais, 1));
                }
                else
                {
                    var index = listCount.FindIndex(x => x.Key == usuario.Pais);
                    listCount[index] = new KeyValuePair<string, int>(usuario.Pais, listCount[index].Value + 1);
                    dict[usuario.Pais].Add(usuario);
                }
            }

            var top5 = listCount.OrderByDescending(c => c.Value).Take(5);

            response.Countries = new List<CountryResponse>();
            foreach (var item in top5)
            {
                var countryResponse = new CountryResponse
                {
                    Country = item.Key,
                    Total = item.Value
                };
                response.Countries.Add(countryResponse);
            }
            response.ExecutionTimeMs = Stopwatch.GetElapsedTime(time1).TotalMilliseconds;
            response.Timestamp = Stopwatch.GetTimestamp();

            return Ok(response);

        }


        [HttpGet]
        [Route("team-insights")]
        public async Task<IActionResult> GetTeamInsights()
        {
            var time1 = Stopwatch.GetTimestamp();
            var response = new TeamInsightResponse();
            if (!_memoryCache.TryGetValue(_cacheKey, out UsuarioModel[]? usuarios))
            {
                return BadRequest("Enviar usuarios antes");
            }

            if (usuarios == null || usuarios.Length == 0)
            {
                return BadRequest("Enviar usuarios antes");
            }

            var dict = new Dictionary<string, TeamResponse>();

            foreach (var usu in usuarios)
            {
                if(!dict.ContainsKey(usu.Equipe.Nome))
                {
                    var tempTeam = new TeamResponse { Team = usu.Equipe.Nome, TotalMembers = 0, Leaders = 0, CompletedProjects = 0, TotalProjects = 0, TotalMembersActive = 0, ActivePercentage = 0 };
                    dict[usu.Equipe.Nome] = tempTeam;
                }
                var team = dict[usu.Equipe.Nome];
                team.TotalMembers++;
                if (usu.Equipe.Lider)
                {
                    team.Leaders++;
                }
                if (usu.Ativo)
                {
                    team.TotalMembersActive++;
                }
                foreach(var proj in usu.Equipe.Projetos)
                {
                    if (proj.Concluido)
                    {
                        team.CompletedProjects++;
                    }
                    team.TotalProjects++;
                }

                team.ActivePercentage = (float)team.TotalMembersActive / team.TotalMembers * 100;
            }

            response.Teams = dict.Select(d => d.Value).ToList();
            response.ExecutionTimeMs = Stopwatch.GetElapsedTime(time1).TotalMilliseconds;
            response.Timestamp = Stopwatch.GetTimestamp();

            return Ok(response);

        }

        [HttpGet]
        [Route("active-users-per-day")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var time1 = Stopwatch.GetTimestamp();
            var response = new ActiveUsersResponse();
            if (!_memoryCache.TryGetValue(_cacheKey, out UsuarioModel[]? usuarios))
            {
                return BadRequest("Enviar usuarios antes");

            }

            if (usuarios == null || usuarios.Length == 0)
            {
                return BadRequest("Enviar usuarios antes");
            }

            foreach (var usu in usuarios) { 
                foreach(var login in usu.Logs)
                {
                    var curDate = response.Logins.FirstOrDefault(lo => lo.Date == DateOnly.Parse(login.Data.Date.ToString("yyyy-MM-dd")));
                    if (curDate == null)
                    {
                        curDate = new ActiveResponse { Date = DateOnly.Parse(login.Data.Date.ToString("yyyy-MM-dd")), Total = 1 };
                        response.Logins.Add(curDate);
                    } else
                    {
                        curDate.Total++;
                    }

                }
                
            
            }

            response.ExecutionTimeMs = Stopwatch.GetElapsedTime(time1).TotalMilliseconds;
            response.Timestamp = Stopwatch.GetTimestamp();

            return Ok(response);

        }


    }
}
