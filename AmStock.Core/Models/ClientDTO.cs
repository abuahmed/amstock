using System.Collections.Generic;

namespace AMStock.Core.Models
{
    public class ClientDTO : CommonFieldsD
    {
        public ClientDTO()
        {
            Organizations = new HashSet<OrganizationDTO>();
        }
        
        public bool HasOnlineAccess
        {
            get { return GetValue(() => HasOnlineAccess); }
            set { SetValue(() => HasOnlineAccess, value); }
        }

        public ICollection<OrganizationDTO> Organizations
        {
            get { return GetValue(() => Organizations); }
            set { SetValue(() => Organizations, value); }
        }
    }
}
