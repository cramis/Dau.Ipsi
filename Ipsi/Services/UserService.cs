using System.Collections.Generic;
using DapperRepository;
using Ipsi.Models;

namespace Ipsi.Services
{
    public interface IUserService
    {
        List<Sua030DbModel> GetUserList(string year, string div, string tp, string sbj);

        Sua030DbModel GetUser(string year, string seatNum);

    }

    public class TestUserService : IUserService
    {
        private List<Sua030DbModel> TestUsers;
        public TestUserService()
        {
            TestUsers = new List<Sua030DbModel>();

            Sua030DbModel user1 = new Sua030DbModel()
            {
                SUA030_YEAR = "2019",
                SUA030_SELECT_DIV = "21",
                SUA030_SELECT_DIVNM = "수시",
                SUA030_SELECT_TP = "60",
                SUA030_SELECT_TPNM = "교과성적우수자",
                SUA030_FIRST_SBJCD = "010",
                SUA030_SBJNM = "인문학과",
                SUA030_SEAT_NUM = "010001",
                SUA030_KOR_NM = "홍길동"
            };

            Sua030DbModel user2 = new Sua030DbModel()
            {
                SUA030_YEAR = "2019",
                SUA030_SELECT_DIV = "21",
                SUA030_SELECT_DIVNM = "수시",
                SUA030_SELECT_TP = "60",
                SUA030_SELECT_TPNM = "교과성적우수자",
                SUA030_FIRST_SBJCD = "010",
                SUA030_SBJNM = "인문학과",
                SUA030_SEAT_NUM = "010002",
                SUA030_KOR_NM = "김말동"
            };

            TestUsers.Add(user1);
            TestUsers.Add(user2);



        }

        public Sua030DbModel GetUser(string year, string seatNum)
        {
            return TestUsers.Find(x => x.SUA030_YEAR == year && x.SUA030_SEAT_NUM == seatNum);
        }

        public List<Sua030DbModel> GetUserList(string year, string div, string tp, string sbj)
        {
            return TestUsers.FindAll(x => x.SUA030_YEAR == year && x.SUA030_SELECT_DIV == div && x.SUA030_SELECT_TP == tp && x.SUA030_FIRST_SBJCD == sbj);
        }
    }
    public class UserService : IUserService
    {
        private IDapperRepository repo { get; }

        public UserService(IDapperRepository repo)
        {
            this.repo = repo;
        }
        public Sua030DbModel GetUser(string year, string seatNum)
        {
            var user = this.repo.GetItem(new Sua030DbModel() { SUA030_YEAR = year, SUA030_SEAT_NUM = seatNum });

            if (user == null)
            {
                throw new System.Exception("User Not Found");
            }

            return user;
        }

        public List<Sua030DbModel> GetUserList(string year, string div, string tp, string sbj)
        {
            if (string.IsNullOrWhiteSpace(year))
            {
                throw new System.Exception("Year must be entered");
            }

            var list = this.repo.GetList(new Sua030DbModel()
            {
                SUA030_YEAR = year,
                SUA030_SELECT_DIV = div,
                SUA030_SELECT_TP = tp,
                SUA030_FIRST_SBJCD = sbj
            });

            if (list == null)
            {
                throw new System.Exception("Users Not Found");
            }

            return list;
        }
    }
}