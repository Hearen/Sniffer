using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SimpleSniffer.BaseClass
{
    public class MyButton: Button
    {
        public new event EventHandler DoubleClick;
        DateTime clickTime;
        bool isClicked = false;
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (isClicked)
            {
                TimeSpan span = DateTime.Now - clickTime;
                if (span.Milliseconds < SystemInformation.DoubleClickTime)
                {
                    DoubleClick(this, e);
                    isClicked = false;
                }
            }
            else
            {
                isClicked = true;
                clickTime = DateTime.Now;
            }
        }  
    }
}
