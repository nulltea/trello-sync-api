using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace TrelloSync
{
	#region Service

	/// <summary>
	/// </summary>
	public interface ITrelloEntity
	{
		/// <summary>
		/// </summary>
		/// <param name="client"></param>
		void SetRestClient(TrelloClient client);
	}

	/// <summary>
	/// </summary>
	public class TrelloEntity : ITrelloEntity
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

	#endregion

	#region Board

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
		[JsonProperty]
		public bool Closed { get; private set; }
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string IdOrganization { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Url { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		private Dictionary<string, string> labelNames { get; set; }

		/// <summary>
		/// </summary>
		[JsonIgnore]
		public Dictionary<string, Label> OnBoardLabels => labelNames.Where(label => !string.IsNullOrEmpty(label.Value))
			.ToDictionary(label => label.Value, label => new Label{Name = label.Key, Color = _trelloColorMapping[label.Key]});

		/// <summary>
		/// </summary>
		public Lists Lists => new Lists(RestClient, this);

		/// <summary>
		/// </summary>
		public List<CustomField> CustomFields => RestClient.Request<List<CustomField>>(new CustomFieldsForBoardRequest(this));

		/// <summary>
		/// </summary>
		public Members Members => new Members(RestClient, this);

		private readonly Dictionary<string, Color> _trelloColorMapping = new Dictionary<string, Color>
		{
			{"green", Color.Green},
			{"yellow", Color.Yellow},
			{"orange", Color.Orange},
			{"red", Color.Red},
			{"purple", Color.Purple},
			{"blue", Color.Blue},
			{"sky", Color.SkyBlue},
			{"lime", Color.Lime},
			{"pink", Color.Pink},
			{"black", Color.Black}
		};
	}

	#endregion

	#region List

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
		[JsonProperty]
		public string BoardId { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public int Position { get; private set; }

		/// <summary>
		/// </summary>
		public Cards Cards => new Cards(RestClient, this);
	}

	#endregion

	#region Card

	/// <summary>
	/// Trello card
	/// </summary>
	public class Card : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty("Id")]
		public string CardId { get; private set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty("Desc")]
		public string Description { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public bool Closed { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty("IdList")]
		public string ListId { get; internal set; }

		/// <summary>
		/// </summary>
		[JsonProperty("IdBoard")]
		public string BoardId { get; internal set; }

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
		public string СustomerProject
		{
			get
			{
				if (!CustomFieldItems.Any()) return string.Empty;
				var customField = Board.CustomFields.FirstOrDefault(field => field.Name == "Project");
				if (customField == null) return string.Empty;
				var fieldItem = CustomFieldItems.FirstOrDefault(item => item.IdCustomField == customField.Id);
				return fieldItem?.Value[customField.Type] ?? string.Empty;
			}
			set
			{
				var customField = Board.CustomFields.FirstOrDefault(field => field.Name == "Project");
				if (customField == null) return;
				RestClient.Request(new CustomFieldItemAddRequest(Convert.ToString(value), this, customField.Id));
			}
		}

		/// <summary>
		/// </summary>
		[JsonIgnore]
		public Labels Labels => new Labels(RestClient, this);

		/// <summary>
		/// </summary>
		[JsonIgnore]
		public CheckLists Checklists => new CheckLists(RestClient, this);

		/// <summary>
		/// </summary>
		[JsonIgnore]
		public Attachments Attachments => new Attachments(RestClient, this);

		/// <summary>
		/// </summary>
		[JsonIgnore]
		public Members Members => new Members(RestClient, this);

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Url { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string ShortUrl { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public DateTime DateLastActivity { get; private set; }
	}

	
	#region Additional card items

	/// <summary>
	/// </summary>
	public class CustomField
	{
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Id { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string IdModel { get; private set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Type { get; private set; }
	}

	/// <summary>
	/// </summary>
	public class CustomFieldItem
	{
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Id { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public Dictionary<string, string> Value { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string IdCustomField { get; private set; }
	}

	/// <summary>
	/// </summary>
	public class Label : TrelloEntity
	{
		/// <summary>
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }
	}

	#region Attachment

	/// <summary>
	/// </summary>
	public class Attachment : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Id { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string IdMember { get; private set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public DateTime Date { get; private set; }

		/// <summary>
		/// </summary>
		public string FilePath { get; set; }

		/// <summary>
		/// </summary>
		public byte[] Contents { get; set; }
	}

	/// <summary>
	/// </summary>
	public class FileAttachment : Attachment
	{
		/// <summary>
		/// </summary>
		public string FilePath { get; set; }
	}

	/// <summary>
	/// </summary>
	public class UrlAttachment : Attachment
	{
		/// <summary>
		/// </summary>
		public string Url { get; set; }
	}

	/// <summary>
	/// </summary>
	public class BatesAttachment : Attachment
	{
		/// <summary>
		/// </summary>
		public byte[] Contents { get; set; }
	}

	#endregion

	/// <summary>
	/// </summary>
	public class CheckItem : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Id { get; private set; }

		/// <summary>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public int Pos { get; private set; }

		/// <summary>
		/// </summary>
		public bool Checked { get; set; }
	}

	#endregion

	#endregion

	#region Organization & Member
	
	/// <summary>
	/// </summary>
	public class Organization : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Id { get; private set; }
		
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string DisplayName { get; private set; }
		
		/// <summary>
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// </summary>
		public string Desc { get; set; }
		
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Url { get; private set; }
		
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Website { get; private set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string LogoHash { get; private set; }

		/// <summary>
		/// </summary>
		public Members Members => new Members(RestClient, this);

		/// <summary>
		/// </summary>
		public Boards Boards => new Boards(RestClient, this);
	}

	/// <summary>
	/// </summary>
	public class Member : TrelloEntity
	{
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Id { get; private set; }
		
		/// <summary>
		/// </summary>
		public string FullName { get; set; }
		
		/// <summary>
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// </summary>
		public string Bio { get; set; }

		/// <summary>
		/// </summary>
		[JsonProperty]
		public string Url { get; private set; }
		
		/// <summary>
		/// </summary>
		[JsonProperty]
		public string AvatarHash { get; private set; }
		
		/// <summary>
		/// </summary>
		public string Initials { get; set; }

		/// <summary>
		/// </summary>
		public Organizations Organizations => new Organizations(RestClient, this);

		/// <summary>
		/// </summary>
		public Boards Boards => new Boards(RestClient, this);
	}

	#endregion
}