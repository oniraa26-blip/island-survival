using System;
using System.Collections.Generic;
using IslandSurvival.InputSystem;
using UnityEngine;

namespace IslandSurvival.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        readonly Collider[] hits = new Collider[24];
        MobileInput input;
        float radius;
        public IInteractable Current { get; private set; }
        public event Action<IInteractable> TargetChanged;
        public void Initialize(MobileInput input, float radius) { this.input = input; this.radius = radius; }
        void Update()
        {
            var count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, ~0, QueryTriggerInteraction.Collide);
            IInteractable best = null; var bestScore = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var components = hits[i].GetComponentsInParent<MonoBehaviour>();
                foreach (var component in components)
                {
                    var candidate = component as IInteractable;
                    if (candidate == null || !candidate.CanInteract) continue;
                    var score = (component.transform.position - transform.position).sqrMagnitude;
                    if (score < bestScore) { best = candidate; bestScore = score; }
                }
            }
            if (!ReferenceEquals(best, Current)) { Current = best; if (TargetChanged != null) TargetChanged(Current); }
            if (input.InteractPressed && Current != null && Current.CanInteract) Current.Interact();
        }
        public void TriggerCurrent() { if (Current != null && Current.CanInteract) Current.Interact(); }
    }
}
