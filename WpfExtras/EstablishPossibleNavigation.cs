namespace WpfExtras
{
    public class EstablishPossibleNavigation
    {
        private readonly IWizardPage nodeToFind;

        private readonly IWizardPage rootNode;

        private bool found;

        private IWizardPage previous;

        public EstablishPossibleNavigation(IWizardPage rootNode, IWizardPage nodeToFind)
        {
            this.rootNode = rootNode;
            this.nodeToFind = nodeToFind;
        }

        /// <summary>
        /// Gets the first node located, should always be the original node upon which "Accept"
        /// was called.
        /// </summary>
        public IWizardPage First { get; private set; }

        /// <summary>
        /// Gets last node found.
        /// </summary>
        public IWizardPage Last { get; private set; }

        /// <summary>
        /// Gets node immediately after found item.
        /// </summary>
        public IWizardPage Next { get; private set; }

        /// <summary>
        /// Node immediately before found item.
        /// </summary>
        public IWizardPage Previous => found ? previous : null;

        public void Execute()
        {
            Visit(rootNode);
        }

        private void Visit(IWizardPage node)
        {
            if (First == null && node != rootNode)
            {
                First = node;
            }

            // Would update the Last node everytime, but if the only
            // node is the root node, then this will be wrong.
            if (node != rootNode)
            {
                Last = node;
            }

            if (!found)
            {
                if (node.Equals(nodeToFind))
                {
                    found = true;
                }
                else
                {
                    // Whilst not found, keep updating previous, last update
                    // will be the one before the searched for node!
                    if (node != rootNode)
                    {
                        previous = node;
                    }
                }
            }
            else
            {
                if (Next == null)
                {
                    Next = node;
                }
            }

            foreach (var child in node.Children)
            {
                Visit(child);
            }
        }
    }
}