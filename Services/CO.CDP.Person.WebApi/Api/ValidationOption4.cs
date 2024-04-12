using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.Person.WebApi.Api.ValidationSpikeFour
{
    public class NewPersonInputModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters long.")]
        [ValidName]
        public string Name { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120 years.")]
        public int Age { get; set; }

        [EmailAddress(ErrorMessage = "A valid email is required.")]
        public string Email { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private static Dictionary<string, Person> _persons = new Dictionary<string, Person>();

        [HttpPost]
        public IActionResult CreatePerson([FromBody] NewPersonInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = (_persons.Count + 1).ToString();
            var person = new Person
            {
                Id = newId,
                Name = model.Name,
                Age = model.Age,
                Email = model.Email
            };
            _persons.Add(newId, person);
            return CreatedAtAction(nameof(GetPerson), new { id = newId }, person);
        }

        [HttpGet("{id}")]
        public IActionResult GetPerson(string id)
        {
            if (_persons.TryGetValue(id, out var person))
            {
                return Ok(person);
            }
            return NotFound();
        }
    }


    public class ValidNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string name)
            {
                if (Regex.IsMatch(name, "^[a-zA-Z ]+$"))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Name can only contain letters and spaces.");
                }
            }
            return new ValidationResult("Invalid input.");
        }
    }

}