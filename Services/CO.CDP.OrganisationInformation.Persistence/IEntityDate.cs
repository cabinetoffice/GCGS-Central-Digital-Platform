using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IEntityDate
{
    DateTimeOffset CreatedOn { get; set; }
    DateTimeOffset UpdatedOn { get; set; }
}
