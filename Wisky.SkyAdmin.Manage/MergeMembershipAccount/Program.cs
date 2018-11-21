using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeMembershipAccount
{
    class Program
    {
        private static HmsEntities _db = new HmsEntities();
        /// <summary>
        /// Tool hỗ trợ merge tài khoản thẻ cứng có sẵn passio vào thẻ mobile
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Tool hỗ trợ merge tài khoản thẻ cứng có sẵn vào thẻ mobile");
            Console.WriteLine("Mã số thẻ sau khi merge là mã số thẻ cứng");
            Console.WriteLine("Tài khoản sau khi merge được cộng dồn cả 2 thẻ");
            while (true)
            {
                
                Console.Write("Nhập mã số thẻ cứng: ");
                string HardMembershipCardCode = Console.ReadLine();
                var HardMembershipCard = _db.MembershipCards.Where(q => q.MembershipCardCode == HardMembershipCardCode && q.Active == true).FirstOrDefault();
                if(HardMembershipCard == null)
                {
                    Console.WriteLine("Không tìm thấy thẻ cứng!");
                    continue;
                }
                Console.Write("Nhập mã số thẻ mobile: ");
                string MobileMembershipCardCode = Console.ReadLine();
                var MobileMembershipCard = _db.MembershipCards.Where(q => q.MembershipCardCode == MobileMembershipCardCode && q.Active == true).FirstOrDefault();
                if (MobileMembershipCard == null)
                {
                    Console.WriteLine("Không tìm thấy thẻ mobile!");
                    continue;
                }
            }
        }
    }
}
