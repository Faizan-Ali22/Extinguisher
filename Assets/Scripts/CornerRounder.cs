/*
MIT License

Copyright (c) 2019 Kirill Evdokimov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
*/

using UnityEngine;
using UnityEngine.UI;

namespace RoundedCorners
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(MaskableGraphic))]
    [AddComponentMenu("UI/Rounded Corners/Corner Rounder")]
    public class CornerRounder : MonoBehaviour
    {
        private static readonly int HalfSizeProperty =
            Shader.PropertyToID("_halfSize");

        private static readonly int RadiusesProperty =
            Shader.PropertyToID("_r");

        private static readonly int RectPropertiesProperty =
            Shader.PropertyToID("_rect2props");

        private static readonly Vector2 WidthNormal =
            new Vector2(0.7071068f, -0.7071068f);

        private static readonly Vector2 HeightNormal =
            new Vector2(0.7071068f, 0.7071068f);

        [Header("Corner Settings")]
        [Tooltip("Enable this to set each corner independently.")]
        public bool independent = false;

        /*
         * Corner order:
         * X = Top Left
         * Y = Top Right
         * Z = Bottom Right
         * W = Bottom Left
         */
        public Vector4 radiiSerialized = new Vector4(40f, 40f, 40f, 40f);

        private Vector4 normalizedRadii;
        private Vector4 rectProperties;

        private Material runtimeMaterial;
        private MaskableGraphic graphic;
        private RectTransform rectTransform;

        private void Awake()
        {
            Initialize();
            Refresh();
        }

        private void OnEnable()
        {
            Initialize();
            Refresh();
        }

        private void OnValidate()
        {
            ClampRadii();
            Initialize();
            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled && runtimeMaterial != null)
            {
                Refresh();
            }
        }

        private void OnDestroy()
        {
            if (graphic != null && graphic.material == runtimeMaterial)
            {
                graphic.material = null;
            }

            if (runtimeMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(runtimeMaterial);
                else
                    DestroyImmediate(runtimeMaterial);
            }

            runtimeMaterial = null;
            graphic = null;
            rectTransform = null;
        }

        private void Initialize()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (graphic == null)
                graphic = GetComponent<MaskableGraphic>();

            if (runtimeMaterial == null)
            {
                Shader shader = Shader.Find("UI/RoundedCorners/CornerRounder");

                if (shader == null)
                {
                    Debug.LogError(
                        "Could not find shader 'UI/RoundedCorners/CornerRounder'. " +
                        "Make sure the rounded-corner shader is included in the project.",
                        this
                    );

                    return;
                }

                runtimeMaterial = new Material(shader)
                {
                    name = "Rounded Corner Material - " + gameObject.name,
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            if (graphic != null && graphic.material != runtimeMaterial)
            {
                graphic.material = runtimeMaterial;
            }
        }

        public void SetRadii(Vector4 radii)
        {
            radiiSerialized = radii;

            independent =
                !Mathf.Approximately(radii.x, radii.y) ||
                !Mathf.Approximately(radii.x, radii.z) ||
                !Mathf.Approximately(radii.x, radii.w);

            ClampRadii();
            Refresh();
        }

        public void SetRadius(float radius)
        {
            radius = Mathf.Max(0f, radius);
            SetRadii(new Vector4(radius, radius, radius, radius));
        }

        public void Refresh()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (runtimeMaterial == null)
                Initialize();

            if (runtimeMaterial == null || rectTransform == null)
                return;

            Rect rect = rectTransform.rect;
            Vector2 size = rect.size;

            normalizedRadii = GetNormalizedRadii(size);
            RecalculateProperties(size);

            runtimeMaterial.SetVector(
                RectPropertiesProperty,
                rectProperties
            );

            runtimeMaterial.SetVector(
                HalfSizeProperty,
                size * 0.5f
            );

            runtimeMaterial.SetVector(
                RadiusesProperty,
                normalizedRadii
            );
        }

        private Vector4 GetNormalizedRadii(Vector2 size)
        {
            return new Vector4(
                NormalizeRadius(radiiSerialized.x, size),
                NormalizeRadius(radiiSerialized.y, size),
                NormalizeRadius(radiiSerialized.z, size),
                NormalizeRadius(radiiSerialized.w, size)
            );
        }

        private float NormalizeRadius(float radius, Vector2 size)
        {
            radius = Mathf.Max(0f, radius);

            float maximumRadius = Mathf.Min(
                Mathf.Abs(size.x) * 0.5f,
                Mathf.Abs(size.y) * 0.5f
            );

            return Mathf.Min(radius, maximumRadius);
        }

        private void ClampRadii()
        {
            radiiSerialized.x = Mathf.Max(0f, radiiSerialized.x);
            radiiSerialized.y = Mathf.Max(0f, radiiSerialized.y);
            radiiSerialized.z = Mathf.Max(0f, radiiSerialized.z);
            radiiSerialized.w = Mathf.Max(0f, radiiSerialized.w);
        }

        private void RecalculateProperties(Vector2 size)
        {
            Vector2 aVector = new Vector2(
                size.x,
                -size.y + normalizedRadii.x + normalizedRadii.z
            );

            float halfWidth =
                Vector2.Dot(aVector, WidthNormal) * 0.5f;

            rectProperties.z = halfWidth;

            Vector2 bVector = new Vector2(
                size.x,
                size.y - normalizedRadii.w - normalizedRadii.y
            );

            float halfHeight =
                Vector2.Dot(bVector, HeightNormal) * 0.5f;

            rectProperties.w = halfHeight;

            Vector2 efVector = new Vector2(
                size.x - normalizedRadii.x - normalizedRadii.y,
                0f
            );

            Vector2 egVector =
                HeightNormal * Vector2.Dot(efVector, HeightNormal);

            Vector2 ePoint = new Vector2(
                normalizedRadii.x - size.x * 0.5f,
                size.y * 0.5f
            );

            Vector2 origin =
                ePoint +
                egVector +
                WidthNormal * halfWidth +
                HeightNormal * -halfHeight;

            rectProperties.x = origin.x;
            rectProperties.y = origin.y;
        }
    }
}