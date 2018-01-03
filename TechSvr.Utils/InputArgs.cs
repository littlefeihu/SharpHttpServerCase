using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TechSvr.Utils
{
    public class InputArgs
    {

        public string PostBody { get; private set; }
        public string MsgType { get; private set; }
        public string InfName { get; private set; }
        public string InfType { get; private set; }
        public string ValidateId { get; private set; }
        public string Data { get; private set; }
        public string SysType { get; private set; }


        public static InputArgs Create(string postBody, string msgType, string infName, string infType, string validateId, string data, string sysType)
        {

            return new InputArgs
            {
                PostBody = postBody,
                MsgType = msgType,
                InfName = infName,
                InfType = infType,
                ValidateId = validateId,
                Data = data,
                SysType = sysType
            };

        }
    }
}
