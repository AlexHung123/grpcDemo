using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Web.protos;
using GrpcService.Web.data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService.Web.service
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MyEmployeeServicecs : EmployeeService.EmployeeServiceBase
    {
        private readonly ILogger<MyEmployeeServicecs> _logger;
        private readonly JwtTokenValidationService _jwtTokenValidationService;

        public MyEmployeeServicecs(ILogger<MyEmployeeServicecs> logger, JwtTokenValidationService jwtTokenValidationService)
        {
            _logger = logger;
            _jwtTokenValidationService = jwtTokenValidationService;
        }

        [AllowAnonymous]
        public override async Task<TokenResponse> CreateToken(TokenRequest request, ServerCallContext context)
        {
            var usermodel = new UserModel
            {
                UserName = request.Username,
                PassWord = request.Password
            };
            var response = await _jwtTokenValidationService.GenerateTokenAsync(usermodel);
            if (response.Success)
            {
                return new TokenResponse
                {
                    Toke = response.Token,
                    Expiration = Timestamp.FromDateTime(response.Expiration),
                    Success = true
                };
            }

            return new TokenResponse
            {
                Success = false
            };
        }

        public override Task<EmployeeResponse> GetByNo(GetByNoRequest request, ServerCallContext context)
        {
            try
            {
                //var trailer = new Metadata
                //{
                //    {"Filed","No"},
                //    {"Message","Something went wrong...."},
                //};
                //if (true)
                //{
                //    throw new RpcException(new Status(StatusCode.DataLoss,"Data is loss...."), trailer);

                //}

                var md = context.RequestHeaders;
                foreach (var pair in md)
                {
                    _logger.LogInformation($"{pair.Key}: {pair.Value}");
                }


                var employee = InMemoryData.Employees
                    .SingleOrDefault(x => x.No == request.No);
                if (employee != null)
                {
                    var response = new EmployeeResponse
                    {
                        Employee = employee
                    };

                    return Task.FromResult(response);
                }
              
            }
            catch(RpcException re)
            {
                throw;
            }catch(Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(Status.DefaultCancelled, e.Message);
            }
            
           

            throw new Exception($"Employee not find with no: {request.No}");

        }

        public override async Task GetAll(GetAllRequest request, IServerStreamWriter<EmployeeResponse> responseStream, ServerCallContext context)
        {
            foreach (var employee in InMemoryData.Employees)
            {
                await responseStream.WriteAsync(new EmployeeResponse { 
                    Employee = employee
                });
            }
        }

        public override async Task<AddPhotoResponse> AddPhoto(IAsyncStreamReader<AddPhotoRequests> requestStream, ServerCallContext context)
        {
            Metadata md = new Metadata();
            foreach (var pair in md)
            {
                Console.WriteLine($"{pair.Key}:{pair.Value}");
            }

            var data = new List<byte>();
            while (await requestStream.MoveNext())
            {
                Console.WriteLine($"Receive: {requestStream.Current.Data.Length} bytes");
                data.AddRange(requestStream.Current.Data);
            }
            Console.WriteLine($"Receive file with {data.Count} bytes");

            return new AddPhotoResponse
            {
                IsOk = true
            };
        }

        public override async Task SaveAll(IAsyncStreamReader<EmployeeRequest> requestStream, IServerStreamWriter<EmployeeResponse> responseStream, ServerCallContext context)
        {
            while(await requestStream.MoveNext())
            {
                var employee = requestStream.Current.E;
                lock (this)
                {
                    InMemoryData.Employees.Add(employee);
                }
                await responseStream.WriteAsync(new EmployeeResponse
                {
                    Employee = employee
                });
            }

            Console.WriteLine("Employee:");
            foreach (var employee in InMemoryData.Employees)
            {
                Console.WriteLine(employee);
            }
        }



    }
}
