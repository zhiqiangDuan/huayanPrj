using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace serverForm
{
    public class Jb_data
    {
        public int cmd;
        public string strType;
        public int number1;
        public int number2;
        public int number3;//命令号
        public Jb_data(int cmd,string type, int num1, int num2,int num3)
        {
            this.cmd = cmd;
            this.strType = type;
            this.number1 = num1;
            this.number2 = num2;
            this.number3 = num2;
        }
    }
}
