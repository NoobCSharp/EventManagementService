using Application.Dtos.UserDtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers
{
    /// <summary>
    /// Контроллер авторизации и регистрации пользователей.  
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Метод регистрации нового пользователя.
        /// </summary>
        /// <param name="request">Данные для регистрации пользователя</param>
        /// <response code="200">Пользователь успешно зарегистрирован</response>
        /// <response code="400">Ошибка регистрации пользователя (пользователь уже существует)</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            await _userService.RegisterUserAsync(request);

            return Ok();
        }

        /// <summary>
        /// Метод авторизации пользователя. 
        /// При успешной авторизации возвращает JWT-токен, 
        /// который необходимо использовать для доступа к защищенным ресурсам сервиса.
        /// </summary>
        /// <param name="request">Данные пользователя для авторизации</param>
        /// <response code="200">Пользователь успешно авторизован</response>
        /// <response code="400">Данные авторизации не верны</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> Login([FromBody] LoginUserRequest request)
        {
            var token = await _userService.LoginUserAsync(request);

            return Ok(token);
        }
    }
}
