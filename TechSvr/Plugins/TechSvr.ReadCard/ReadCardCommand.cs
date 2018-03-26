using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.ReadCard
{
    public class ReadCardCommand : ICommand
    {
        public string Name { get { return "readcard"; } }

        public ResposeMessage Excute(InputArgs input)
        {
            return new ResposeMessage
            {
                type = ResultType.SUCCESS.ToString(),
                message = "读卡",
                messageCode = MessageCode.information.ToString(),
                data = new
                {
                    CardType = "0 ",
                    PatName = "张三",
                    Age = "27",
                    Sex = "女",
                    BirthDay = "1990-01-01",
                    Nation = "汉",
                    IdNum = "222222222222222222",
                    Address = "合肥蜀山区长江西路23楼卫宁健康"
                }

            };
        }
    }
}
