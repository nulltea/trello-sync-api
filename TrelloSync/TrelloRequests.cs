using System;
using System.Collections;
using System.Collections.Generic;
using RestSharp;

namespace TrelloSync
{
	internal class CardsAddRequest : RestRequest
	{
		public CardsAddRequest(Card card) : base("cards", Method.POST)
		{
			if(string.IsNullOrEmpty(card.Name)) return;
			if(string.IsNullOrEmpty(card.ListId)) return;

			AddParameter("name", card.Name);
			AddParameter("idList", card.ListId);
			if (card.DueDate.HasValue) AddParameter("due", card.DueDate);
			if (!string.IsNullOrEmpty(card.Description)) AddParameter("desc", card.Description);
		}
	}

	internal class CardsRequest : RestRequest
	{
		public CardsRequest(string cardId, string resource = "", Method method = Method.GET) : base("cards/{cardId}/" + resource, method)
		{
			AddParameter("cardId", cardId, ParameterType.UrlSegment);
			AddParameter("labels", "true");
			AddParameter("badges", "true");
			AddParameter("checkItemStates", "true");
			AddParameter("attachments", "true");
			AddParameter("checklists", "all");
		}
	}

	internal class ListsRequest : RestRequest
	{
		public ListsRequest(string listId, string resource = "", Method method = Method.GET) : base("list/{listId}/" + resource, method)
		{
			AddParameter("listId", listId, ParameterType.UrlSegment);
		}
	}

	internal class CardsForListRequest : ListsRequest
	{
		public CardsForListRequest(string listId) : base(listId, "cards")
		{
			AddParameter("labels", "true");
			AddParameter("badges", "true");
			AddParameter("checkItemStates", "true");
			AddParameter("attachments", "true");
			AddParameter("checklists", "all");
			AddParameter("customFieldItems", "true");
		}
	}

	internal class MembersRequest : RestRequest
	{
		public MembersRequest(string memberIdOrUsername, string resource = "") : base("members/{memberIdOrUsername}/" + resource)
		{
			AddParameter("memberIdOrUsername", memberIdOrUsername, ParameterType.UrlSegment);
		}
	}

	internal class BoardsForMemberRequest : MembersRequest
	{
		public BoardsForMemberRequest(string memberIdOrUsername) : base(memberIdOrUsername, "boards") { }
	}

	internal class BoardsRequest : RestRequest
	{
		public BoardsRequest(string boardId, string resource = "", Method method = Method.GET) : base("boards/{boardId}/" + resource, method)
		{
			AddParameter("boardId", boardId, ParameterType.UrlSegment);
		}
	}


	internal class ListsForBoardRequest : BoardsRequest
	{
		public ListsForBoardRequest(Board board) : base(board.BoardId, "lists") { }
	}

	internal class CustomFieldsForBoardRequest : BoardsRequest
	{
		public CustomFieldsForBoardRequest(Board board) : base(board.BoardId, "customFields") { }
	}

	/// <summary>
	/// Request type in the body of which must be added application key and auth credentials
	/// RestSharp does not this for some reasones
	/// </summary>
	internal class PutBodyRequest : RestRequest
	{
		internal Dictionary<string, object> BodyObject;

		public PutBodyRequest(string resource, Method method) : base(resource, method)
		{
			BodyObject = new Dictionary<string, object>();
		}
	}

	internal class CustomFieldItemAddRequest : PutBodyRequest
	{
		public CustomFieldItemAddRequest(object value, Card card, string fieldId) : base("cards/{cardId}/customField/{fieldId}/item", Method.PUT)
		{
			AddParameter("cardId", card.CardId, ParameterType.UrlSegment);
			AddParameter("fieldId", fieldId, ParameterType.UrlSegment);
			switch (value)
			{
				case int _:
				case long _:
				case decimal _:
					BodyObject.Add("value", new {number = Convert.ToString(value)});
					break;
				case bool _:
					BodyObject.Add("value", new {@checked = Convert.ToString(value)});
					break;
				case string _:
					BodyObject.Add("value", new {text = Convert.ToString(value)});
					break;
				case IEnumerable _:
					BodyObject.Add("value", new {list = Convert.ToString(value)});
					break;
				case DateTime _:
					BodyObject.Add("value", new {date = Convert.ToString(value)});
					break;
				default: 
					BodyObject.Add("value", new {text = Convert.ToString(value)});
					break;
			}
		}
	}
}