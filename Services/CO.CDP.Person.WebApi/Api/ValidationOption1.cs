using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace CO.CDP.Person.WebApi.Api.ValidationSpikeOne
{
    public class Person
    {
        [Required(ErrorMessage = "ID is required.")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "ID must be numeric.")]
        public string Id { get; init; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; init; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; init; }

        [Required]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; }
    }

    public class NewPerson
    {
        [Required(AllowEmptyStrings = true)]
        public string Name { get; init; }
        public int Age { get; init; }
        [EmailAddress]
        public string Email { get; init; }
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

        [HttpPost]
        public IActionResult CreatePerson([FromBody] NewPerson newPerson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = (_persons.Keys.Select(int.Parse).Max() + 1).ToString();
            var person = new Person
            {
                Id = newId,
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
    }
}
