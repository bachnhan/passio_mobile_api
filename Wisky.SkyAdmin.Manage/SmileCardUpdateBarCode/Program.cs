//using SmileCardUpdateBarCode.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmileCardUpdateBarCode
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool check = false;
            check = ApiSmileCard.SendRequestSmileCard();
            if(check)
            {
                Console.WriteLine("Gửi thành công");
            }
            else
            {
                Console.WriteLine("Gửi chưa thành công");
            }
        }
    }
}
