using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class EditorExtension : Object
	{
		protected EditorExtension(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected EditorExtension(AssetInfo assetInfo, uint hideFlags):
			base(assetInfo, hideFlags)
		{
		}

		/// <summary>
		/// Not Release and Not Prefab
		/// </summary>
		public static bool IsReadPrefabParentObject(TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsForPrefab();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (IsReadPrefabParentObject(reader.Flags))
			{
				PrefabParentObject.Read(reader);
				PrefabInternal.Read(reader);
			}
#endif
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

#if UNIVERSAL
			yield return PrefabParentObject.FetchDependency(file);
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(PrefabParentObjectName, GetPrefabParentObject(container.Flags).ExportYAML(container));
			node.Add(PrefabInternalName, GetPrefabInternal(container).ExportYAML(container));
			return node;
		}

		private PPtr<EditorExtension> GetPrefabParentObject(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadPrefabParentObject(flags))
			{
				return PrefabParentObject;
			}
#endif
			return default;
		}
		private PPtr<Prefab> GetPrefabInternal(IExportContainer container)
		{
#if UNIVERSAL
			if (IsReadPrefabParentObject(container.Flags))
			{
				return PrefabInternal;
			}
#endif
			if (container.ExportFlags.IsForPrefab())
			{
				PrefabExportCollection prefabCollection = (PrefabExportCollection)container.CurrentCollection;
				return prefabCollection.Asset.File.CreatePPtr((Prefab)prefabCollection.Asset);
			}
			return default;
		}

		public const string PrefabParentObjectName = "m_PrefabParentObject";
		public const string PrefabInternalName = "m_PrefabInternal";
		public const string CorrespondingSourceObjectName = "m_CorrespondingSourceObject";
		public const string PrefabInstanceName = "m_PrefabInstance";
		public const string PrefabAssetName = "m_PrefabAsset";

#if UNIVERSAL
		/// <summary>
		/// CorrespondingSourceObject later
		/// </summary>
		public PPtr<EditorExtension> PrefabParentObject;
		/// <summary>
		/// Prefab previously
		/// </summary>
		public PPtr<Prefab> PrefabInternal;
#endif
	}
}
