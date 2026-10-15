using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragathiApi.Application.Dtos
{
    public record SubmitResponse(int LeadId, bool AlreadyReceived);
}
