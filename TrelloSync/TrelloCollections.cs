using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace TrelloSync
{
		/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TrelloCollection<T> :  List<T>, ITrelloEntity where T: ITrelloEntity
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
			ForEach(item => item.SetRestClient(restClient));
		}

		public TrelloCollection() { } 
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
		public Card this[string pld] => this.FirstOrDefault(card => Convert.ToString(card.InnerSystemTaskId) == pld);
	}

	#region Additional collections
	
	/// <summary>
	/// </summary>
	public class Labels : TrelloCollection<Label>
	{
		private readonly Card _parentCard;

		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="card"></param>
		public Labels(TrelloClient restClient, Card card)
			: base(restClient, restClient.Request<List<Label>>(new GetLabelsRequest(card)))
		{
			_parentCard = card;
		}

		/// <summary>
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>
		public new void Add(Label label)
		{
			RestClient.Request(new AddLabelRequest(_parentCard, label));
		}

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public Label this[string name] => this.FirstOrDefault(label => label.Name == name);

		/// <summary>
		/// </summary>
		/// <param name="color"></param>
		public Label this[Color color] => this.FirstOrDefault(label => label.Color == color);
	}

	/// <summary>
	/// </summary>
	public class Attachments : TrelloCollection<Attachment>
	{
		private readonly Card _parentCard;

		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="card"></param>
		public Attachments(TrelloClient restClient, Card card)
			: base(restClient, restClient.Request<List<Attachment>>(new GetAttachmentsRequest(card)))
		{
			_parentCard = card;
		}

		/// <summary>
		/// </summary>
		/// <param name="attachment"></param>
		/// <returns></returns>
		public new void Add(Attachment attachment)
		{
			RestClient.Request(new AddAttachmentRequest(_parentCard, attachment));
		}

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public Attachment this[string name] => this.FirstOrDefault(attachment => attachment.Name == name);
	}

	/// <summary>
	/// </summary>
	public class CheckLists : TrelloCollection<CheckList>
	{
		private readonly Card _parentCard;

		/// <summary>
		/// </summary>
		/// <param name="restClient"></param>
		/// <param name="card"></param>
		public CheckLists(TrelloClient restClient, Card card)
			: base(restClient, restClient.Request<List<CheckList>>(new GetCheckListsRequest(card)))
		{
			_parentCard = card;
		}

		/// <summary>
		/// </summary>
		/// <param name="checkList"></param>
		/// <returns></returns>
		public new void Add(CheckList checkList)
		{
			RestClient.Request(new AddCheckListRequest(_parentCard, checkList));
		}

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public CheckList this[string name] => this.FirstOrDefault(checkList => checkList.Name == name);
	}


	/// <summary>
	/// </summary>
	[JsonObject]
	public class CheckList : TrelloCollection<CheckItem>
	{
		/// <summary>
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		public int Pos { get; set; }

		[JsonProperty]
		internal List<CheckItem> CheckItems
		{
			set => AddRange(value);
		}

		/// <summary>
		/// </summary>
		/// <param name="checkItem"></param>
		/// <returns></returns>
		public new void Add(CheckItem checkItem)
		{
			RestClient.Request(new AddCheckItemRequest(this, checkItem));
		}

		/// <summary>
		/// </summary>
		/// <param name="pos"></param>
		public new CheckItem this[int pos] => this.FirstOrDefault(card => card.Pos == pos);

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		public CheckItem this[string name] => this.FirstOrDefault(card => card.Name == name);
	}

	#endregion
}