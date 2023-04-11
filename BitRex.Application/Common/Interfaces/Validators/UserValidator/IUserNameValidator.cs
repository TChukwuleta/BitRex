using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces.Validators.UserValidator
{
    public interface IUserNameValidator : IBaseValidator
    {
        public string Name { get; set; }
    }
}
