using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Api.ErrorSpikeTwo
{
    public record Person
    {
        [Required(AllowEmptyStrings = true)] public required string Id { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        public required int Age { get; init; }
        [EmailAddress] public required string Email { get; init; }
    }

    public record NewPerson
    {
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        public required int Age { get; init; }
        [EmailAddress] public required string Email { get; init; }
    }

    public record UpdatedPerson
    {
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        public required int Age { get; init; }
        [EmailAddress] public required string Email { get; init; }
    }


    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private static readonly Dictionary<string, Person> _persons = Enumerable.Range(1, 5)
            .ToDictionary(index => index.ToString(), index => new Person
            {
                Id = index.ToString(),
                Name = $"Sussan Tables {index}",
                Age = 40 + index,
                Email = "sussan@example.com"
            });

        [HttpGet]
        public IActionResult GetAllPersons()
        {
            return Ok(_persons.Values);
        }

        [HttpPost]
        [ValidateModelAttribute]
        public IActionResult CreatePerson([FromBody] NewPerson newPerson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = new Person
            {
                Id = $"{_persons.Count + 1}",
                Name = newPerson.Name,
                Age = newPerson.Age,
                Email = newPerson.Email
            };
            _persons.Add(person.Id, person);
            return CreatedAtAction(nameof(GetPerson), new { personId = person.Id }, person);
        }

        [HttpGet("{personId}")]
        public IActionResult GetPerson(string personId)
        {
            if (_persons.TryGetValue(personId, out var person))
            {
                return Ok(person);
            }
            return NotFound();
        }

        [HttpPut("{personId}")]
        [ValidateModelAttribute]
        public IActionResult UpdatePerson(string personId, [FromBody] UpdatedPerson updatedPerson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_persons.ContainsKey(personId))
            {
                _persons[personId] = new Person
                {
                    Id = personId,
                    Name = updatedPerson.Name,
                    Age = updatedPerson.Age,
                    Email = updatedPerson.Email
                };
                return Ok(_persons[personId]);
            }
            return NotFound();
        }

        [HttpDelete("{personId}")]
        public IActionResult DeletePerson(string personId)
        {
            if (_persons.Remove(personId))
            {
                return NoContent();
            }
            return NotFound();
        }
    }


    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }

    public static class ApiExtensions
    {
        public static void DocumentPersonApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0.0",
                Title = "Person API",
                Description = "",
            });
        }
    }
}