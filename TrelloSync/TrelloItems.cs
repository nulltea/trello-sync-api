using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace TrelloSync
{
	/// <summary>
	/// </summary>
	public class TrelloEntity
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
	}

	/// <summary>
	/// Trello Board
	/// </summary>
	public class Board : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty("Id")]
		public string BoardId { get; set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		
		/// <summary>
		/// </summary>
		[JsonProperty("Desc")]
		public string Description { get; set; }

		/// <summary>
		/// </summary>
		public bool Closed { get; set; }
		/// <summary>
		/// </summary>
		public string IdOrganization { get; set; }
		/// <summary>
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// </summary>
		[JsonIgnore]
		public Dictionary<Color, string> LabelNames { get; set; }

		/// <summary>
		/// </summary>
		public Lists Lists => new Lists(RestClient, this);

		/// <summary>
		/// </summary>
		public List<CustomField> CustomFields => RestClient.Request<List<CustomField>>(new CustomFieldsForBoardRequest(this));
	}

	/// <summary>
	/// Trello list
	/// </summary>
	public class List : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty("Id")]
		public string ListId { get; set; }
		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		public string BoardId { get; set; }

		/// <summary>
		/// </summary>
		public double Position { get; set; }

		/// <summary>
		/// </summary>
		public Cards Cards => new Cards(RestClient, this);
	}

	/// <summary>
	/// Trello card
	/// </summary>
	public class Card : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty("Id")]
		public string CardId { get; set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty("Desc")]
		public string Description { get; set; }

		/// <summary>
		/// </summary>
		public bool Closed { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty("IdList")]
		public string ListId { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty("IdBoard")]
		public string BoardId { get; set; }

		/// <summary>
		/// </summary>
		public Board Board => RestClient.Request<Board>(new BoardsRequest(BoardId));

		/// <summary>
		/// </summary>
		[JsonProperty("Due")]
		public DateTime? DueDate { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty("customFieldItems")]
		public List<CustomFieldItem> CustomFieldItems { get; set; }

		/// <summary>
		/// </summary>
		public int InnerSystemTaskId
		{
			get
			{
				if (!CustomFieldItems.Any()) return 0;
				var customField = Board.CustomFields.FirstOrDefault(field => field.Name == "PLD");
				if (customField == null) return 0;
				var fieldItem = CustomFieldItems.FirstOrDefault(item => item.IdCustomField == customField.Id);
				return fieldItem != null ? Convert.ToInt32(fieldItem.Value[customField.Type]) : 0;
			}
			set
			{
				var customField = Board.CustomFields.FirstOrDefault(field => field.Name == "PLD");
				if (customField == null) return;
				RestClient.Request(new CustomFieldItemAddRequest(Convert.ToString(value), this, customField.Id));
			}
		}

		/// <summary>
		/// </summary>
		public List<Label> Labels { get; set; }

		//TODO public List<Checklist> Checklists { get; set; }

		/// <summary>
		/// </summary>
		public List<Attachment> Attachments { get; set; }

		/// <summary>
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// </summary>
		public string ShortUrl { get; set; }

		/// <summary>
		/// </summary>
		public DateTime DateLastActivity { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty("IdMembers")]
		public List<string> MemberId { get; set; }
	}

	public class CustomField
	{
		public string Id { get; set; }
		public string IdModel { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
	}

	public class CustomFieldItem
	{
		public string Id { get; set; }

		public Dictionary<string, string> Value { get; set; }

		public string IdCustomField { get; set; }
	}

	public class Label
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	public class Attachment
	{
		public string Id { get; set; }
		public string IdMember { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public DateTime Date { get; set; }
	}
}