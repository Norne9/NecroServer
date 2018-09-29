using Unosquare.Labs.EmbedIO.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.EmbedIO;
using System.Threading.Tasks;
using MasterReqResp;

namespace NecroMaster
{
    public class ClientController : WebApiController
    {
        public ClientController(IHttpContext context) : base(context)
        {

        }

        [WebApiHandler(HttpVerbs.Post, "/client/register")]
        public bool RegisterUser()
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                return this.JsonExceptionResponse(e);
            }
        }
    }
}
