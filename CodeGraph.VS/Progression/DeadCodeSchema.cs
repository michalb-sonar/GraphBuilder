using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.GraphModel;

namespace CodeGraph.VS.Progression
{
    public static partial class DeadCodeSchema
    {
        public const string Name = "Dead code graph schema";

        public static GraphSchema Schema { get; private set; }

        static DeadCodeSchema()
        {
            GraphSchema schema = new GraphSchema(Name);
            Schema = schema;
        }

        public static class NodeCategories
        {
            static NodeCategories()
            {
                DeadMethodCategory = DeadCodeSchema.Schema.RegisterNodeCategory("DeadCodeSchema_DeadMethodCategory", "DeadMethodCategory");
                ZombieMethodCategory = DeadCodeSchema.Schema.RegisterNodeCategory("DeadCodeSchema_ZombieMethodCategory", "ZombieMethodCategory");
            }

            public static GraphCategory DeadMethodCategory { get; private set; }

            public static GraphCategory ZombieMethodCategory { get; private set; }
        }

        public static class LinkCategories
        {
            static LinkCategories()
            {
                ZombieCallLinkCategory = DeadCodeSchema.Schema.RegisterLinkCategory("DeadCodeSchema_ZombieCallLinkCategory", "ZombieCallLinkCategory", false);
            }

            public static GraphCategory ZombieCallLinkCategory { get; private set; }

        }

        #region Schema helper methods

        internal static GraphCategory RegisterLinkCategory(this GraphSchema schema, string id, string label, bool isContainment)
        {
            return schema.Categories.AddNewCategory(id, delegate
            {
                GraphMetadata metadata = new GraphMetadata(label, null, null, GraphMetadataOptions.Serializable | GraphMetadataOptions.Browsable | GraphMetadataOptions.Removable);
                if (isContainment)
                {
                    metadata[GraphCommonSchema.IsContainment] = true;
                }
                return metadata;
            });
        }

        internal static GraphCategory RegisterNodeCategory(this GraphSchema schema, string id, string label)
        {
            return schema.RegisterNodeCategory(id, label, null);
        }

        internal static GraphCategory RegisterNodeCategory(this GraphSchema schema, string id, string label, GraphCategory basedOn)
        {
            GraphCategory category = schema.Categories.AddNewCategory(id, () => new GraphMetadata(label, null, null, GraphMetadataOptions.Default));
            if (basedOn != null)
            {
                category.BasedOnCategory = basedOn;
            }
            return category;
        }

        internal static GraphProperty RegisterProperty(this GraphSchema schema, string id, string label, Type type, GraphMetadataOptions options)
        {
            return schema.Properties.AddNewProperty(id, type, () => new GraphMetadata(label, null, null, options));
        }

        #endregion

    }
}
