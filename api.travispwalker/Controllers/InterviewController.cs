using Library.Database.Auth;
using Library.Database.Auth.Models;
using Library.Database.TableModels;
using Library.Database.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace api.travispwalker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewController : Controller
    {
        private readonly UserManagerExtension _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _applicationDbContext;

        public InterviewController(
           UserManagerExtension userManager,
           RoleManager<IdentityRole> roleManager,
           IConfiguration configuration,
           ITokenService tokenService,
           ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _applicationDbContext = applicationDbContext;
        }

        [HttpPost]
        [Route("insert-update-question")]
        [Authorize]
        public async Task<IActionResult> InsertUpdateQuestion([FromBody] InterviewQuestion model)
        {
            InterviewPrepQuestion? question;
            if(model.QuestionId== null)
            {
                //New Question
                if(String.IsNullOrWhiteSpace(model.Answer)) return BadRequest("You must supply an answer to the question");
                if (String.IsNullOrWhiteSpace(model.Question)) return BadRequest("You Must supply text for the question");

                question = new InterviewPrepQuestion();
                question.InterviewQuestionId = $"{Guid.NewGuid()}";
                question.Question = model.Question;
                question.Answer = model.Answer;
                question.AddedOn = DateTime.Now;
                question.AddedByUser = _userManager.GetUserAsync(User).Result;

                InterviewPrepQuestion.InsertOrUpdate(question, _applicationDbContext, User);

                return Ok(question);
            }
            else
            {
                question = InterviewPrepQuestion.GetById(model.QuestionId, _applicationDbContext, User);
                if(question != null)
                {
                    //Existing Question
                    if (String.IsNullOrWhiteSpace(model.Answer)) return BadRequest("You must supply an answer to the question");
                    if (String.IsNullOrWhiteSpace(model.Question)) return BadRequest("You Must supply text for the question");

                    question.Question = model.Question;
                    question.Answer = model.Answer;

                    return Ok(question);
                }
                else
                {
                    return BadRequest(model);
                }
                
            }
        }

        [HttpPost]
        [Route("return-question-by-id")]
        [Authorize]
        public async Task<IActionResult> ReturnQuestionByID([FromBody] InterviewQuestion model)
        {
            InterviewPrepQuestion? question;
            if (!String.IsNullOrWhiteSpace(model.QuestionId))
            {
                question = InterviewPrepQuestion.GetById(model.QuestionId, _applicationDbContext, User);
                if (question != null)
                {
                    model.Question = question.Question;
                    model.Answer = question.Answer;

                    return Ok(model);
                }
                else
                {
                    return BadRequest("QuestionId not found");
                }
            }

            return BadRequest("QuestionId not provided or not found");
        }

        [HttpGet]
        [Route("get-random-question")]
        [Authorize]
        public async Task<IActionResult> GetRandomQuestion()
        {
            InterviewPrepQuestion? question = await Task.FromResult(InterviewPrepQuestion.GetRandom(_applicationDbContext)) ;
            return Ok(question);
        }

        [HttpGet]
        [Route("get-all-questions")]
        [Authorize]
        public async Task<IActionResult> GetAllQuestions()
        {
            InterviewPrepQuestion[] questions = await Task.FromResult(InterviewPrepQuestion.GetAll(_applicationDbContext));
            return Ok(questions);
        }

    }
}
