using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.Styles;

namespace CodeGraph.VS.Progression
{
    partial class DeadCodeSchema
    {
        public static Graph CreateGraph()
        {
            Graph graph = new Graph();
            ApplyStyles(graph);
            return graph;
        }

        public static void ApplyStyles(Graph graph)
        {
            AddDeadMethdCategoryNodeStyle(graph);
            AddZombieMethodCategoryNodeStyle(graph);
            AddZombieCallCategoryLinkStyle(graph);
        }

        private static void AddDeadMethdCategoryNodeStyle(Graph graph)
        {
            // Define the basic style properties
            GraphConditionalStyle style = new GraphConditionalStyle(graph);
            style.TargetType = typeof(GraphNode);
            style.GroupLabel = "Dead Method";
            style.ValueLabel = "Dead method";
            style.ToolTip = "Dead method";

            // Apply the condition
            GraphCondition condition = new GraphCondition(style);
            condition.Expression = "HasCategory('DeadCodeSchema_DeadMethodCategory')";
            style.Conditions.Add(condition);

            // Apply the setters
            GraphSetter setter;

            setter = new GraphSetter(style, "Background");
            setter.Value = "#FF8080FF";
            style.Setters.Add(setter);
            graph.Styles.Add(style);
        }

        private static void AddZombieMethodCategoryNodeStyle(Graph graph)
        {
            // Define the basic style properties
            GraphConditionalStyle style = new GraphConditionalStyle(graph);
            style.TargetType = typeof(GraphNode);
            style.GroupLabel = "ZombieMethod";
            style.ValueLabel = "ZombieMethod";
            style.ToolTip = "Zombie method";

            // Apply the condition
            GraphCondition condition = new GraphCondition(style);
            condition.Expression = "HasCategory('DeadCodeSchema_ZombieMethodCategory')";
            style.Conditions.Add(condition);

            // Apply the setters
            GraphSetter setter;

            setter = new GraphSetter(style, "Background");
            setter.Value = "#FFC0C0C0";
            style.Setters.Add(setter);
            graph.Styles.Add(style);
        }

        private static void AddZombieCallCategoryLinkStyle(Graph graph)
        {
            // Define the basic style properties
            GraphConditionalStyle style = new GraphConditionalStyle(graph);
            style.TargetType = typeof(GraphLink);
            style.GroupLabel = "Zombie Call";
            style.ValueLabel = "Zombie Call";
            style.ToolTip = "Zombie call link";

            // Apply the condition
            GraphCondition condition = new GraphCondition(style);
            condition.Expression = "HasCategory('DeadCodeSchema_ZombieCallLinkCategory')";
            style.Conditions.Add(condition);

            // Apply the setters
            GraphSetter setter;

            setter = new GraphSetter(style, "Stroke");
            setter.Value = "#FFC0C0C0";
            style.Setters.Add(setter);
            graph.Styles.Add(style);
        }

    }
}