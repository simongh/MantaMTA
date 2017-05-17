using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds details relating to how an email was handled (e.g. a bounce or abuse report).  If it was a
	/// bounce, information about how it was identified as a bounce is included.
	/// /// </summary>
	public class EmailProcessingDetails
	{
		public EmailProcessingDetails()
		{
			// Start .ProcessingResult as NotYetSet as some methods use EmailProcessingDetails objects, but are too low
			// level to set this property.
			this.ProcessingResult = EmailProcessingResult.NotYetSet;
		}

		/// <summary>
		/// Compares two EmailProcessingDetails objects to see if they are equal.
		/// </summary>
		/// <param name="obj">Another EmailProcessingDetails object to compare to this one.</param>
		/// <returns>true if the two objects have the same value, else false.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is EmailProcessingDetails))
				return false;

			EmailProcessingDetails otherObj = obj as EmailProcessingDetails;

			if (this.ProcessingResult == otherObj.ProcessingResult && this.BounceIdentifier == otherObj.BounceIdentifier && this.MatchingBounceRuleID == otherObj.MatchingBounceRuleID && this.MatchingValue == otherObj.MatchingValue)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Returns a HashCode representing the values of the object's properties.
		/// When overriding Equals(), the compiler displays a warning if GetHashCode() isn't also overriden so here we are.
		/// </summary>
		/// <returns>The HashCode for this object.</returns>
		public override int GetHashCode()
		{
			if (this == null)
				return 0;

			return (this.ProcessingResult.GetHashCode() + this.BounceIdentifier.GetHashCode() + this.MatchingBounceRuleID.GetHashCode() +
				(this.MatchingValue == null ? 0 : this.MatchingValue.GetHashCode())
			);
		}

		/// <summary>
		/// Overridden ToString() so that we can more easily see what values are being held when debugging.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string temp = String.Format("ProResult: {0}, BounceIdent: {1}, BounceRuleID: {2}, MatchingValue: {3}", this.ProcessingResult, this.BounceIdentifier, this.MatchingBounceRuleID,
				(string.IsNullOrWhiteSpace(this.MatchingValue) ? "(blank)" : (this.MatchingValue.Length < 10 ? this.MatchingValue : this.MatchingValue.Substring(0, 10)))
			);

			return temp;
		}

		/// <summary>
		/// Indicates whether the email was successfully processed or if there was an issue, perhaps
		/// with its content.
		/// </summary>
		public EmailProcessingResult ProcessingResult { get; set; }

		/// <summary>
		/// Indicates the type of information that positively identified the email as a bounce.
		/// </summary>
		public BounceIdentifier BounceIdentifier { get; set; }

		/// <summary>
		/// If the .BounceIdentifier property is set to BounceRule, then this will be the RuleID of the matching Bounce Rule.
		/// </summary>
		public int MatchingBounceRuleID { get; set; }

		/// <summary>
		/// The value in the email that was used to identify the bounce.
		/// When .BounceIdentifier is BounceRule, this will be the Crtieria value of that Rule,
		/// when .BounceIdentifier is NdrCode or SmtpCode, this will be the code, e.g. "550" or "4.4.7".
		/// </summary>
		public string MatchingValue { get; set; }
	}
}