namespace PironGames.MLLib.Samples.SelfDrivingCars.Editor
{
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.UIElements;

    public class NNGraphView : GraphView
    {
        private Blackboard m_bb;

        public NNGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            m_bb = new Blackboard(this);
            m_bb.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            m_bb.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            Add(m_bb);
        }

        //public override Blackboard GetBlackboard()
        //{
        //    var result = new Blackboard(this);

        //    //result.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        //    result.style.width = new StyleLength(new Length(400, LengthUnit.Pixel));
        //    result.style.height = new StyleLength(new Length(200, LengthUnit.Pixel));

        //    /* COMMENTED OUT BLOCK #1
        //    var c = result.capabilities;
        //    result.capabilities = c & ~Capabilities.Movable;
        //    */
        //    /* COMMENTED OUT BLOCK #2
        //    result.RegisterCallback<MouseDownEvent>(OnMouseDown);
        //    result.RegisterCallback<MouseUpEvent>(OnMouseUp);
        //    result.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        //    */
        //    return result;
        //}

        //public override void ReleaseBlackboard(Blackboard toRelease)
        //{
        //    toRelease.RemoveFromHierarchy();
        //    /* COMMENTED OUT BLOCK #3
        //    toRelease.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        //    toRelease.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        //    toRelease.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        //    */
        //}

        //private static void OnMouseDown(MouseDownEvent e)
        //{
        //    e.StopImmediatePropagation();
        //}

        //private static void OnMouseUp(MouseUpEvent e)
        //{
        //    e.StopImmediatePropagation();
        //}

        //private static void OnMouseMove(MouseMoveEvent e)
        //{
        //    e.StopImmediatePropagation();
        //}
    }
}