using System;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.AutoFormatOnSave
{
    public class EnableAutoFormatOnSaveCommandHandler: CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Enabled = true;
            info.Checked = Settings.AutoFormatOnSave;
        }

        protected override void Run()
        {
            Settings.AutoFormatOnSave = !Settings.AutoFormatOnSave;
        }
    }
}
