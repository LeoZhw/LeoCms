using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leo.Microservice.Zookeeper.Routing
{
    public class ServiceRoute
    {
        /// <summary>
        /// 服务可用地址。
        /// </summary>
        public IEnumerable<string> Address { get; set; }
        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceRouteDescriptor serviceRouteDescriptor { get; set; }

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var model = obj as ServiceRoute;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.serviceRouteDescriptor != serviceRouteDescriptor)
                return false;

            return model.Address.Count() == Address.Count() && model.Address.All(addressModel => Address.Contains(addressModel));
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ServiceRoute model1, ServiceRoute model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceRoute model1, ServiceRoute model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}
