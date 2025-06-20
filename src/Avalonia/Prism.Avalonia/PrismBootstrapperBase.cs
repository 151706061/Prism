using System;
using Avalonia;
using Avalonia.Controls;
using Prism.Common;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace Prism
{
    /// <summary>
    /// Base class that provides a basic bootstrapping sequence and hooks
    /// that specific implementations can override
    /// </summary>
    /// <remarks>
    /// This class must be overridden to provide application specific configuration.
    /// </remarks>
    public abstract class PrismBootstrapperBase
    {
        private IContainerExtension _containerExtension;
        private IModuleCatalog _moduleCatalog;

        /// <summary>
        /// The dependency injection container used to resolve objects
        /// </summary>
        public IContainerProvider Container => _containerExtension;

        /// <summary>
        /// Gets the shell user interface
        /// </summary>
        /// <value>The shell user interface.</value>
        protected AvaloniaObject Shell { get; set; }

        /// <summary>
        /// Runs the bootstrapper process.
        /// </summary>
        public void Run()
        {
            ConfigureViewModelLocator();
            Initialize();
            OnInitialized();
        }

        /// <summary>
        /// Configures the <see cref="Prism.Mvvm.ViewModelLocator"/> used by Prism.
        /// </summary>
        protected virtual void ConfigureViewModelLocator()
        {
            PrismInitializationExtensions.ConfigureViewModelLocator();
        }

        /// <summary>
        /// Runs the initialization sequence to configure the Prism application.
        /// </summary>
        protected virtual void Initialize()
        {
            ContainerLocator.SetContainerExtension(CreateContainerExtension());
            _containerExtension = ContainerLocator.Current;
            _moduleCatalog = CreateModuleCatalog();
            RegisterRequiredTypes(_containerExtension);
            RegisterTypes(_containerExtension);

            ConfigureModuleCatalog(_moduleCatalog);

            var regionAdapterMappings = _containerExtension.Resolve<RegionAdapterMappings>();
            ConfigureRegionAdapterMappings(regionAdapterMappings);

            var defaultRegionBehaviors = _containerExtension.Resolve<IRegionBehaviorFactory>();
            ConfigureDefaultRegionBehaviors(defaultRegionBehaviors);

            RegisterFrameworkExceptionTypes();

            var shell = CreateShell();
            if (shell != null)
            {
                MvvmHelpers.AutowireViewModel(shell);
                RegionManager.SetRegionManager(shell, _containerExtension.Resolve<IRegionManager>());
                RegionManager.UpdateRegions();
                InitializeShell(shell);
            }

            InitializeModules();
        }

        /// <summary>
        /// Creates the container used by Prism.
        /// </summary>
        /// <returns>The container</returns>
        protected abstract IContainerExtension CreateContainerExtension();

        /// <summary>
        /// Creates the <see cref="IModuleCatalog"/> used by Prism.
        /// </summary>
        ///  <remarks>
        /// The base implementation returns a new ModuleCatalog.
        /// </remarks>
        protected virtual IModuleCatalog CreateModuleCatalog()
        {
            return new ModuleCatalog();
        }

        /// <summary>
        /// Registers all types that are required by Prism to function with the container.
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected virtual void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            if (_moduleCatalog == null)
                throw new InvalidOperationException("IModuleCatalog was null");

            containerRegistry.RegisterRequiredTypes(_moduleCatalog);
        }

        /// <summary>
        /// Used to register types with the container that will be used by your application.
        /// </summary>
        protected abstract void RegisterTypes(IContainerRegistry containerRegistry);

        /// <summary>
        /// Configures the <see cref="IRegionBehaviorFactory"/>.
        /// This will be the list of default behaviors that will be added to a region.
        /// </summary>
        protected virtual void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors)
        {
            regionBehaviors?.RegisterDefaultRegionBehaviors();
        }

        /// <summary>
        /// Configures the default region adapter mappings to use in the application, in order
        /// to adapt UI controls defined in XAML to use a region and register it automatically.
        /// May be overwritten in a derived class to add specific mappings required by the application.
        /// </summary>
        /// <returns>The <see cref="RegionAdapterMappings"/> instance containing all the mappings.</returns>
        protected virtual void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            regionAdapterMappings?.RegisterDefaultRegionAdapterMappings();
        }

        /// <summary>
        /// Registers the <see cref="Type"/>s of the Exceptions that are not considered
        /// root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected virtual void RegisterFrameworkExceptionTypes()
        {
        }

        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// <returns>The shell of the application.</returns>
        protected abstract AvaloniaObject CreateShell();

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        protected virtual void InitializeShell(AvaloniaObject shell)
        {
            Shell = shell;
        }

        /// <summary>
        /// Contains actions that should occur last.
        /// </summary>
        protected virtual void OnInitialized()
        {
            if (Shell is Window window)
                window.Show();
        }

        /// <summary>
        /// Configures the <see cref="IModuleCatalog"/> used by Prism.
        /// </summary>
        protected virtual void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) { }

        /// <summary>
        /// Initializes the modules.
        /// </summary>
        protected virtual void InitializeModules()
        {
            PrismInitializationExtensions.RunModuleManager(Container);
        }
    }
}
