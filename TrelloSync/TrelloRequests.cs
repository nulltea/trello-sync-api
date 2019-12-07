using System;
using System.Collections;
using System.Collections.Generic;
using RestSharp;

namespace TrelloSync
{
	#region Board Requests

	internal class BoardsForMemberRequest : MembersRequest
	{
		public BoardsForMemberRequest(Member member) : base(member.Username, "boards") { }
	}

	internal class BoardsRequest : RestRequest
	{
		public BoardsRequest(string boardId, string resource = "", Method method = Method.GET) : base("boards/{boardId}/" + resource, method)
		{
			AddParameter("boardId", boardId, ParameterType.UrlSegment);
		}
	}

	internal class CustomFieldsForBoardRequest : BoardsRequest
	{
		public CustomFieldsForBoardRequest(Board board) : base(board.BoardId, "customFields") { }
	}

	internal class LabelsForBoardRequest : BoardsRequest
	{
		public LabelsForBoardRequest(Board board) : base(board.BoardId, "labels") { }
	}

	internal class ByBoardMembersRequest : BoardsRequest
	{
		public ByBoardMembersRequest(Board member) : base(member.BoardId, "members") { }
	}

	#endregion

	#region List Requests

	internal class ListsRequest : RestRequest
	{
		public ListsRequest(string listId, string resource = "", Method method = Method.GET) : base("list/{listId}/" + resource, method)
		{
			AddParameter("listId", listId, ParameterType.UrlSegment);
		}
	}

	internal class ListsForBoardRequest : BoardsRequest
	{
		public ListsForBoardRequest(Board board) : base(board.BoardId, "lists") { }
	}

	#endregion

	#region Card Requests

	internal class CardsAddRequest : RestRequest
	{
		public CardsAddRequest(Card card) : base("cards", Method.POST)
		{
			if(string.IsNullOrEmpty(card.Name)) return;
			if(string.IsNullOrEmpty(card.ListId)) return;

			AddParameter("name", card.Name);
			AddParameter("idList", card.ListId);
			if (card.DueDate.HasValue) AddParameter("due", card.DueDate.Value.ToString("yyyy.MM.dd"));
			if (!string.IsNullOrEmpty(card.Description)) AddParameter("desc", card.Description);
		}
	}

	internal class CardsRequest : RestRequest
	{
		public CardsRequest(string cardId, string resource = "", Method method = Method.GET) : base("cards/{cardId}/" + resource, method)
		{
			AddParameter("cardId", cardId, ParameterType.UrlSegment);
			AddParameter("badges", "true");
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

	internal class ByCardMembersRequest : CardsRequest
	{
		public ByCardMembersRequest(Card card) : base(card.CardId, "members") { }
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

	internal class GetLabelsRequest : CardsRequest
	{
		public GetLabelsRequest(Card card) : base(card.CardId, "labels") { }
	}

	internal class AddLabelRequest : CardsRequest
	{
		public AddLabelRequest(Card card, Label label) : base(card.CardId, "labels", Method.POST)
		{
			AddParameter("value", label.Name.ToLower());
		}
	}

	internal class GetAttachmentsRequest : CardsRequest
	{
		public GetAttachmentsRequest(Card card) : base(card.CardId, "attachments") { }
	}

	internal class AddAttachmentRequest : CardsRequest
	{
		public AddAttachmentRequest(Card card, Attachment attachment) : base(card.CardId, "attachments", Method.POST)
		{
			AddParameter("name", attachment.Name);
			switch (attachment)
			{
				case FileAttachment file:
					AddFile("file", file.FilePath);
					break;
				case UrlAttachment url:
					AddParameter("url", url.Url);
					break;
				case BatesAttachment bytes:
					AddFile("file", bytes.Contents, attachment.Name);
					break;
			}
		}
	}

	internal class GetCheckListsRequest : CardsRequest
	{
		public GetCheckListsRequest(Card card) : base(card.CardId, "checklists") { }
	}

	internal class AddCheckListRequest : RestRequest
	{
		public AddCheckListRequest(Card card, CheckList checkList) : base("checklists", Method.POST)
		{
			AddParameter("idCard", card.CardId);
			AddParameter("name", card.Name);
		}
	}

	internal class AddCheckItemRequest : RestRequest
	{
		public AddCheckItemRequest(CheckList checkList, CheckItem checkItem) : base("checklists/{checklistId}/checkItems", Method.POST)
		{
			AddParameter("checklistId", checkList.Id);
			AddParameter("name", checkItem.Name);
			AddParameter("checked", checkItem.Checked);
		}
	}

	#endregion

	#region Organization & Member Requests

	internal class MembersRequest : RestRequest
	{
		public MembersRequest(string memberIdOrUsername, string resource = "") : base("members/{memberIdOrUsername}/" + resource)
		{
			AddParameter("memberIdOrUsername", memberIdOrUsername, ParameterType.UrlSegment);
		}
	}

	internal class ByMemberOrganizationsRequest : MembersRequest
	{
		public ByMemberOrganizationsRequest(Member member) : base(member.Username, "organizations") { }
	}

	internal class OrganizationRequest : RestRequest
	{
		public OrganizationRequest(string orgNameOrId, string resource = "") : base("organization/{orgId}/" + resource)
		{
			AddParameter("orgId", orgNameOrId, ParameterType.UrlSegment);
		}
	}

	internal class ByOrganizationMembers : OrganizationRequest
	{
		public ByOrganizationMembers(Organization organization) : base(organization.Name, "members") { }
	}
	internal class ByOrganizationBoards : OrganizationRequest
	{
		public ByOrganizationBoards(Organization organization) : base(organization.Name, "boards") { }
	}

	#endregion
}