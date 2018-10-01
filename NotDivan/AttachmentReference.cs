namespace NotDivan
{
    /// <summary>
    /// represents an attachment to a master document
    /// </summary>
    public class AttachmentReference
    {
        /// <summary>
        /// attachment document ID
        /// </summary>
        public string AttachmentId { get; set; }

        /// <summary>
        /// name of attachment as appended to master document
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// size, in bytes, of attachment
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// sequence number, for sequential attachments?
        /// </summary>
        public int SequenceNumber { get; set; }
    }
}
