using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;


namespace ScriptCs.Rebus.Hosting.ScriptHandlers.WebApi
{
	public class AssemblyControllerTypeResolver : DefaultHttpControllerTypeResolver
	{
		private ICollection<Type> _controllerTypes;

		internal AssemblyControllerTypeResolver(ICollection<Type> controllerTypes)
		{
			_controllerTypes = controllerTypes;
		}

		public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
		{
			return _controllerTypes;
		}

		internal static IEnumerable<Type> WhereControllerType(IEnumerable<Type> types)
		{
			return types.Where(x => typeof(System.Web.Http.Controllers.IHttpController).IsAssignableFrom(x));
		}

		internal static bool AllAssignableToIHttpController(IEnumerable<Type> types)
		{
			return types.All(x => typeof(System.Web.Http.Controllers.IHttpController).IsAssignableFrom(x));
		}
	}
}