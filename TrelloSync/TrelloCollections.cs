using System.Collections.Generic;
using System.Linq;

namespace TrelloSync
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TrelloCollection<T> : List<T> where T: TrelloEntity
	{
		/// <summary>
		/// </summary>
		protected TrelloClient RestClient { get; set; }

		/// <summary>
		/// </summary>
		/// <param name="client"></param>
		public void SetRestClient(TrelloClient client)
		{
			RestClient = client;
		}

		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="entities"></param>
		public TrelloCollection(TrelloClient restClient, List<T> entities) : base(entities)
		{
			RestClient = restClient;
			ForEach(list => list.SetRestClient(restClient));
		}
	}

	/// <summary>
	/// </summary>
	public class Boards : TrelloCollection<Board>
	{
		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="memberUserName"></param>
		public Boards(TrelloClient restClient, string memberUserName) 
			: base(restClient, restClient.Request<List<Board>>(new BoardsForMemberRequest(memberUserName))) { }

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Board this[string name] => this.FirstOrDefault(board => board.Name == name);
	}

	/// <summary>
	/// </summary>
	public class Lists : TrelloCollection<List>
	{
		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="board"></param>
		public Lists(TrelloClient restClient, Board board) 
			: base(restClient, restClient.Request<List<List>>(new ListsForBoardRequest(board))) { }

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public List this[string name] => this.FirstOrDefault(board => board.Name == name);
	}

	/// <summary>
	/// </summary>
	public class Cards : TrelloCollection<Card>
	{
		private readonly List _parentList;

		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="list"></param>
		public Cards(TrelloClient restClient, List list)
			: base(restClient, restClient.Request<List<Card>>(new CardsForListRequest(list.ListId)))
		{
			_parentList = list;
		}

		/// <summary>
		/// </summary>
		/// <param name="card"></param>
		/// <returns></returns>
		public new Card Add(Card card)
		{
			card.BoardId = _parentList.BoardId;
			card.ListId = _parentList.ListId;
			return RestClient.Request<Card>(new CardsAddRequest(card));
		}

		/// <summary>
		/// </summary>
		/// <param name="pld"></param>
		public Card this[string pld] => this.FirstOrDefault(card => Text.Convert(card.InnerSystemTaskId) == pld);
	}
}