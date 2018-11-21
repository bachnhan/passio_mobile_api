using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ITokenUserService
    {
        bool UpdateUserToken(string username, string token);
    }
    public partial class TokenUserService
    {
        public bool UpdateUserToken(string username, string token)
        {
            try
            {
                //mai mot cap nhat lai extraHour, tam thoi code cung so 4
                var lifetime = 4;

                //check xem user da ton tai chua, co thi update token, chua thi tao moi
                var existedUser = this.Get(a => a.Username == username).FirstOrDefault();
                var time = Utils.GetCurrentDateTime();
                if (existedUser != null) //user da ton tai, update
                {
                    existedUser.CreateTime = time;
                    existedUser.EndTime = time.AddHours(lifetime);
                    existedUser.Token = token;
                    this.Update(existedUser);
                }
                else //user chua ton tai, tao moi
                {
                    TokenUser tokenUser = new TokenUser()
                    {
                        Username = username,
                        Token = token,
                        CreateTime = time,
                        EndTime = time.AddHours(lifetime)
                    };
                    this.Create(tokenUser);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }
    }
}
