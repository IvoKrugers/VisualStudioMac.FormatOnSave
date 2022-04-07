using System;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.AutoFormatOnSave
{
    public class StartupCommandHandler: CommandHandler
    {
        protected override void Run()
        {
            AutoFormatOnSaveHandler.Instance.Startup();
        }
    }
}
