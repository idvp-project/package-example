using UnityEngine;
using System.Collections.Generic;
using Assets.Platform.Scripts.Animation;
using U = iDVP.Platform.Entity;

public class CircleDiagramControlScript : ControlElementView<U.CircleDiagramControl>
{
    private CircleSectorControl mCircle = null;

    private float mValueAnimationTimeline;
    private AnimatedFloat mValueAnimation;

    private CircleSectorControl.CircleSectorDescription[] to = null;
    private CircleSectorControl.CircleSectorDescription[] from = null;
    private CircleSectorControl.CircleSectorDescription[] curr = null;
    
    private void Awake()
    {
        mCircle = GetComponentInChildren<CircleSectorControl>();
        mValueAnimation = new AnimatedFloat(() => mValueAnimationTimeline, OnValueAnimationTick);
        mValueAnimation.TargetAnimationTime = 2.5f;
    }

    private void Update()
    {
        mValueAnimation.Update();
    }

    private bool IsReadyForContinue(List<U.CircleDiagramColorSector> sectors)
    {
        if (curr == null || curr.Length != sectors.Count)
            return false;

        for (int i = 0; i < sectors.Count; ++i)
        {
            var a = curr[i];
            var b = sectors[i];

            if (a.color1 != b.Color1.ToColor())
                return false;

            if (a.color2 != b.Color2.ToColor())
                return false;

            if (a.radius != b.Radius)
                return false;
        }

        return true;
    }

    private void RefreshDiagramData(List<U.CircleDiagramColorSector> sectors)
    {
        if (IsReadyForContinue(sectors) != true)
        {
            curr = new CircleSectorControl.CircleSectorDescription[sectors.Count];
            from = new CircleSectorControl.CircleSectorDescription[sectors.Count];
            for (int i = 0; i < sectors.Count; ++i)
            {
                var desc = sectors[i];
                curr[i] = new CircleSectorControl.CircleSectorDescription()
                {
                    color1 = desc.Color1.ToColor(),
                    color2 = desc.Color2.ToColor(),
                    value = 0f,
                    radius = desc.Radius
                };

                from[i] = new CircleSectorControl.CircleSectorDescription()
                {
                    color1 = desc.Color1.ToColor(),
                    color2 = desc.Color2.ToColor(),
                    value = 0f,
                    radius = desc.Radius
                };
            }
        }
        else
        {
            from = curr;
        }

        to = new CircleSectorControl.CircleSectorDescription[sectors.Count];
        for (int i = 0; i < sectors.Count; ++i)
        {
            var sector = sectors[i];
            to[i] = new CircleSectorControl.CircleSectorDescription()
            {
                color1 = sector.Color1.ToColor(),
                color2 = sector.Color2.ToColor(),
                value = sector.Value,
                radius = sector.Radius
            };
        }

        //здесь нужно проверить, если предыдущие или текущие значения в сумме дают ноль
        //то просто показываем диаграмму, без проигрывания анимации
        var valueFrom = GetSectorsValueSum(from);
        var valueTo = GetSectorsValueSum(to);

        if (valueFrom != 0f && valueTo != 0f)
        {
            mValueAnimation.StartFromToAnimation(0f, 1f);
        } else
        {
            curr = to;
            mCircle.BuildCircleDiagram(to);
        }
    }

    private float GetSectorsValueSum(CircleSectorControl.CircleSectorDescription[] sectors)
    {
        float value = 0f;
        if (sectors != null)
        {
            for (int i = 0; i < sectors.Length; ++i)
                value += sectors[i].value;
        }
        return value;
    }

    private void OnValueAnimationTick(float time)
    {
        mValueAnimationTimeline = time;

        for (int i = 0; i < to.Length; ++i)
        {
            curr[i].value = Mathf.Lerp(from[i].value, to[i].value, time);
        }

        mCircle.BuildCircleDiagram(curr);
    }

    protected override void OnSetupView(U.CircleDiagramControl element)
    {
        base.OnSetupView(element);

        var maxRadius = float.MinValue;
        for (int i = 0; i < element.Sectors.Count; ++i)
        {
            maxRadius = Mathf.Max(maxRadius, element.Sectors[i].Radius);
        }

        var srcBounds = new Bounds(Vector3.zero, new Vector3(maxRadius, maxRadius, 0f) * 2f);
        var dstBounds = new Bounds(ActualSize / 2f, ActualSize);

        mCircle.transform.AlignTransformBoundsToBounds(srcBounds, dstBounds);
        mValueAnimation.TargetAnimationTime = element.AnimationTime;


        var sectors = ValidateSectors(element.Sectors);
        RefreshDiagramData(sectors);
    }

    private List<U.CircleDiagramColorSector> ValidateSectors(List<U.CircleDiagramColorSector> sectors)
    {
        if (sectors != null && sectors.Count != 0)
        {
            var sumValue = 0f;
            for (int i = 0; i < sectors.Count; ++i)
                sumValue += sectors[i].Value;
            if (sumValue!=0f)
            {
                return sectors;
            }
        }

        var grayShit = new U.CircleDiagramColorSector()
        {
            Radius = 1f,
            Color1 = new U.ColorARGB() { A = 255, R = 127, G = 127, B = 127 },
            Color2 = new U.ColorARGB() { A = 255, R = 127, G = 127, B = 127 },
            Value = 1f,
            Title = "default",
        };

        Debug.Log("make grayshit");
        return new List<U.CircleDiagramColorSector>(new[] { grayShit });
    }
}