using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace TrelloSync
{
	/// <summary>
	/// </summary>
	public class Trello
	{
		private readonly TrelloClient _restClient;

		/// <summary>
		/// </summary>
		/// <param name="key"></param>
		public Trello(string key)
		{
			_restClient = new TrelloClient(key);
		}

		/// <summary>
		/// </summary>
		public Boards Boards => new Boards(_restClient, "me");

		/// <summary>
		/// </summary>
		/// <param name="token"></param>
		public void Authorize(string token)
		{
			_restClient.Authenticate(token);
		}

		/// <summary>
		/// </summary>
		public void Deauthorize()
		{
			_restClient.Authenticate(null);
		}
	}

	/// <summary>
	/// </summary>
	public class TrelloClient : RestClient
	{
		private readonly string _applicationKey;
		private const string TrelloBaseUrl = "https://trello.com/1";

		/// <summary>
		/// </summary>
		/// <param name="applicationKey"></param>
		public TrelloClient(string applicationKey) : base(TrelloBaseUrl)
		{
			_applicationKey = applicationKey;
			this.AddDefaultParameter("key", applicationKey);
			AddHandler("application/json", () => new TrelloDeserializer());
		}

		/// <summary>
		/// </summary>
		/// <param name="memberToken"></param>
		public void Authenticate(string memberToken)
		{
			Authenticator = memberToken == null ? null : new MemberTokenAuthenticator(memberToken);
		}

		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		public void Request(IRestRequest request)
		{
			handleUnauthorizedPutRequest(request);
			var response = Execute(request);
			ThrowIfRequestWasUnsuccessful(request, response);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request"></param>
		/// <returns></returns>
		public T Request<T>(IRestRequest request) where T : class, new()
		{
			handleUnauthorizedPutRequest(request);

			var response = Execute<T>(request);
			ThrowIfRequestWasUnsuccessful(request, response);

			if (response.StatusCode == HttpStatusCode.NotFound) return null;
			if (response.Data is TrelloEntity trelloEntity) trelloEntity.SetRestClient(this);

			return response.Data;
		}

		/// <summary>
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public Task RequestAsync(IRestRequest request)
		{
			handleUnauthorizedPutRequest(request);
			var taskCompletionSource = new TaskCompletionSource<object>();
			ExecuteAsync(request, (response, handle) =>
			{
				try
				{
					ThrowIfRequestWasUnsuccessful(request, response);
					taskCompletionSource.SetResult(null);
				}
				catch (Exception e)
				{
					taskCompletionSource.SetException(e);
				}
			});

			return taskCompletionSource.Task;
		}

		private void ThrowIfRequestWasUnsuccessful(IRestRequest request, IRestResponse response)
		{
			if (request.Method == Method.GET && response.StatusCode == HttpStatusCode.NotFound) return;
			if (response.StatusCode == HttpStatusCode.Unauthorized) throw new AuthenticationException(response.Content);
			if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.Content);
		}

		/// <summary>
		/// Add application key and auth credentials in put request body
		/// </summary>
		/// <param name="request"></param>
		private void handleUnauthorizedPutRequest(IRestRequest request)
		{
			if (request is PutBodyRequest putBodyRequest)
			{
				putBodyRequest.BodyObject.Add("key", _applicationKey);
				putBodyRequest.BodyObject.Add("token", (Authenticator as MemberTokenAuthenticator)?.Token);
				putBodyRequest.AddJsonBody(putBodyRequest.BodyObject);
			}
		}
	}

	internal class TrelloDeserializer : IDeserializer
	{		
		public T Deserialize<T>(IRestResponse response)
		{
			return JsonConvert.DeserializeObject<T>(response.Content);
		}

		// We have some abstraction leakage here since we don't care about these things.
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public string DateFormat { get; set; }
	}

	internal class MemberTokenAuthenticator : IAuthenticator
	{
		internal readonly string Token;

		public MemberTokenAuthenticator(string token)
		{
			Token = token;
		}

		public void Authenticate(IRestClient client, IRestRequest request)
		{
			request.AddParameter("token", Token);
		}
	}
}