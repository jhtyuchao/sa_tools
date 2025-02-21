﻿using SharpDX;
using SharpDX.Direct3D9;
using SAModel;
using SAModel.Direct3D;
using SAModel.SAEditorCommon.DataTypes;
using SAModel.SAEditorCommon.SETEditing;
using System;
using System.Collections.Generic;
using BoundingSphere = SAModel.BoundingSphere;
using Mesh = SAModel.Direct3D.Mesh;
using SplitTools;
using SAModel.SAEditorCommon;

namespace SA2ObjectDefinitions.Common
{
	public abstract class ItemBoxBase : ObjectDefinition
	{
		protected NJS_OBJECT model;
		protected Mesh[] meshes;
		protected int childindex;
		protected UInt16 ItemBoxLength = 11;
		protected TexnameArray texarr;
		protected Texture[] texs;

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			transform.Push();
			transform.NJTranslate(item.Position);
			HitResult result = model.CheckHit(Near, Far, Viewport, Projection, View, transform, meshes);
			transform.Pop();
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Device dev, EditorCamera camera, MatrixStack transform)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			if (texs == null)
				texs = ObjectHelper.GetTextures("objtex_common", texarr, dev);
			//((BasicAttach)model.Children[childindex].Attach).Material[0].TextureID = itemTexs[Math.Min(Math.Max((int)item.Scale.X, 0), ItemBoxLength)];
			transform.Push();
			transform.NJTranslate(item.Position);
			result.AddRange(model.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, texs, meshes, EditorOptions.IgnoreMaterialColors, EditorOptions.OverrideLighting));
			if (item.Selected)
				result.AddRange(model.DrawModelTreeInvert(transform, meshes));
			transform.Pop();
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			List<ModelTransform> result = new List<ModelTransform>();
			//((BasicAttach)model.Children[childindex].Attach).Material[0].TextureID = itemTexs[Math.Min(Math.Max((int)item.Scale.X, 0), ItemBoxLength)];
			transform.Push();
			transform.NJTranslate(item.Position);
			result.Add(new ModelTransform(model, transform.Top));
			transform.Pop();
			return result;
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			MatrixStack transform = new MatrixStack();
			transform.NJTranslate(item.Position);
			return ObjectHelper.GetModelBounds(model, transform);
		}

		internal int[] itemTexs = { 15, 9, 2, 1, 8, 10, 11, 12, 13, 18, 16 };

		internal int[] charTexs = { 2, 5, 3, 6, 4, 7};

		private readonly PropertySpec[] customProperties = new PropertySpec[] {
			new PropertySpec("Item", typeof(Items), "Extended", null, null, (o) => (Items)Math.Min(Math.Max((int)o.Scale.X, 0), 11), (o, v) => o.Scale.X = (int)v)
		};

		public override PropertySpec[] CustomProperties { get { return customProperties; } }

		public override float DefaultXScale { get { return 0; } }

		public override float DefaultYScale { get { return 0; } }

		public override float DefaultZScale { get { return 0; } }

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);

			return matrix;
		}
	}

	public class ItemBox : ItemBoxBase
	{
		public override void Init(ObjectData data, string name)
		{
			model = ObjectHelper.LoadModel("object/OBJECT_ITEMBOX.sa2mdl");
			meshes = ObjectHelper.GetMeshes(model);
			childindex = 2;
			texarr = new TexnameArray("object/tls/itembox.tls");
		}

		public override void SetOrientation(SETItem item, Vertex direction)
		{
			int x;
			int z;
			direction.GetRotation(out x, out z);
			item.Rotation.X = x + 0x4000;
			item.Rotation.Z = -z;
		}

		public override string Name { get { return "Item Box"; } }
	}

	public class FloatingItemBox : ItemBoxBase
	{
		public override void Init(ObjectData data, string name)
		{
			model = ObjectHelper.LoadModel("object/OBJECT_ITEMBOXAIR.sa2mdl");
			meshes = ObjectHelper.GetMeshes(model);
			childindex = 1;
			texarr = new TexnameArray("object/tls/itemboxair.tls");
		}

		public override string Name { get { return "Floating Item Box"; } }
	}

	public enum Items
	{
		SpeedUp,
		FiveRings,
		ExtraLife,
		TenRings,
		TwentyRings,
		Barrier,
		Bomb,
		HealthPack,
		MagneticBarrier,
		Empty,
		Invincibility,
	}
}