﻿using SharpDX;
using SharpDX.Direct3D9;
using SAModel;
using SAModel.Direct3D;
using SAModel.SAEditorCommon.DataTypes;
using SAModel.SAEditorCommon.SETEditing;
using System.Collections.Generic;
using BoundingSphere = SAModel.BoundingSphere;
using Mesh = SAModel.Direct3D.Mesh;
using SAModel.SAEditorCommon;

namespace SADXObjectDefinitions.Common
{
	class SpinnerA : ObjectDefinition
	{
		private NJS_OBJECT model;
		private Mesh[] meshes;
		private NJS_OBJECT cylmdl;
		private Mesh[] cylmsh;

		public override void Init(ObjectData data, string name)
		{
			model = ObjectHelper.LoadModel("enemy/spinna/models/supi_supi.nja.sa1mdl");
			meshes = ObjectHelper.GetMeshes(model);
			cylmdl = ObjectHelper.LoadModel("nondisp/cylinder01.nja.sa1mdl");
			cylmsh = ObjectHelper.GetMeshes(cylmdl);
		}

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.NJRotateY(item.Rotation.Y);
			HitResult result = model.CheckHit(Near, Far, Viewport, Projection, View, transform, meshes);
			transform.Pop();
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Device dev, EditorCamera camera, MatrixStack transform)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.Push();
			transform.NJRotateY(item.Rotation.Y);
			result.AddRange(model.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("SUPI_SUPI"), meshes, EditorOptions.IgnoreMaterialColors, EditorOptions.OverrideLighting));
			if (item.Selected)
				result.AddRange(model.DrawModelTreeInvert(transform, meshes));
			transform.Pop();
			float sx = (item.Scale.X + 70) * 0.1f;
			transform.NJScale(sx, 0.02f, sx);
			result.AddRange(cylmdl.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, null, cylmsh, boundsByMesh: true));
			transform.Pop();
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			List<ModelTransform> result = new List<ModelTransform>();
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.NJRotateY(item.Rotation.Y);
			result.Add(new ModelTransform(model, transform.Top));
			transform.Pop();
			return result;
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			MatrixStack transform = new MatrixStack();
			transform.NJTranslate(item.Position);
			transform.NJRotateY(item.Rotation.Y);
			return ObjectHelper.GetModelBounds(model, transform);
		}

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);
			MatrixFunctions.RotateY(ref matrix, item.Rotation.Y);

			return matrix;
		}

		public override string Name { get { return "Spinner (Attack)"; } }

		private readonly PropertySpec[] customProperties = new PropertySpec[] {
			new PropertySpec("Search Range", typeof(float), "Extended", null, 0, (o) => o.Scale.X, (o, v) => o.Scale.X = (float)v)
		};

		public override PropertySpec[] CustomProperties { get { return customProperties; } }

		private readonly PropertySpec[] missionProperties = new PropertySpec[] {
			new PropertySpec("Destroy For Mission", typeof(bool), null, null, 0, (o) => ((MissionSETItem)o).PRMBytes[4] != 0, (o, v) => ((MissionSETItem)o).PRMBytes[4] = (byte)((bool)v ? 1 : 0)),
			new PropertySpec("Items Required", typeof(byte), null, null, 0, (o) => ((MissionSETItem)o).PRMBytes[5], (o, v) => ((MissionSETItem)o).PRMBytes[5] = (byte)v)
		};

		public override PropertySpec[] MissionProperties { get { return missionProperties; } }
	}
}
