using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using NHibernate.Validator.Cfg;
using NHibernate.Validator.Cfg.MappingSchema;

namespace NHibernate.Validator.Engine
{
	internal class StateFullClassValidatorFactory : AbstractClassValidatorFactory
	{
		private static readonly IClassMappingFactory defaultClassMappingFactory = new JITClassMappingFactory();

		private IClassMappingFactory classMappingFactory = defaultClassMappingFactory;
		private readonly Dictionary<System.Type, IClassValidator> validators = new Dictionary<System.Type, IClassValidator>();

		public StateFullClassValidatorFactory(ResourceManager resourceManager, CultureInfo culture, IMessageInterpolator userInterpolator, ValidatorMode validatorMode) 
			: base(resourceManager, culture, userInterpolator, validatorMode) {}

		public void Initialize(MappingLoader loader)
		{
			try
			{
				StateFullClassMappingFactory sfcmf = new StateFullClassMappingFactory();
				classMappingFactory = sfcmf;
				foreach (NhvMapping mapping in loader.Mappings)
				{
					foreach (NhvmClass nhvmClass in mapping.@class)
					{
						sfcmf.AddClassDefinition(nhvmClass);
					}
				}
				foreach (System.Type type in sfcmf.GetLoadedDefinitions())
				{
					GetRootValidator(type);
				}
			}
			finally
			{
				classMappingFactory = defaultClassMappingFactory;
			}
		}

		public override IClassMappingFactory ClassMappingFactory
		{
			get { return classMappingFactory; }
		}

		public override IClassValidator GetRootValidator(System.Type type)
		{
			IClassValidator result;
			if(!validators.TryGetValue(type, out result))
			{
				result = new ClassValidator(type, new Dictionary<System.Type, IClassValidator>(), this);
				validators.Add(type, result);
			}
			return result;
		}

		public override void GetChildValidator(IClassValidatorImplementor parentValidator, System.Type childType)
		{
			// TODO : Add an existing validators to child of the parent validator
			new ClassValidator(childType, parentValidator.ChildClassValidators, this);
		}

		public IDictionary<System.Type, IClassValidator> Validators
		{
			get { return validators; }
		}
	}
}