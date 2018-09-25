using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.SystemUI
{
    /// <summary>
    /// 界面命令按钮，沿用arcgis的组织方式
    /// </summary>
    public interface ICommand
    {
        //object BindingControl { get; set; }
        // 摘要: 
        //     The bitmap that is used as the icon on this command.

        int Bitmap { get; }
        //
        // 摘要: 
        //     The caption of this command.

        string Caption { get; }
        //
        // 摘要: 
        //     The name of the category with which this command is associated.

        string Category { get; }
        //
        // 摘要: 
        //     Indicates if this command is checked.

        bool Checked { get; }
        //
        // 摘要: 
        //     Indicates if this command is enabled.

        bool Enabled { get; }
        //
        // 摘要: 
        //     The help context ID associated with this command.

        int HelpContextID { get; }
        //
        // 摘要: 
        //     The name of the help file associated with this command.

        string HelpFile { get; }
        //
        // 摘要: 
        //     The statusbar message for this command.

        string Message { get; }
        //
        // 摘要: 
        //     The name of this commmand.

        string Name { get; }
        //
        // 摘要: 
        //     The tooltip for this command.

        string Tooltip { get; }

        // 摘要: 
        //     Occurs when this command is clicked.
        void OnClick();
        //
        // 摘要: 
        //     Occurs when this command is created.
        void OnCreate(object Hook);
    }
}
