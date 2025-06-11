using System.Collections.Generic;
using System.Linq;
using Movement3D.ComboGraph;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class ComboReader : Feature
    {
        private Attack attack;
        private List<string> bufferSignal;
        private ComboContainer _graph;
        
        private NodeData _actualCombo;
        private List<NodeLinkData> _transitions;
        private NodeLinkData _transition;
        
        public FullAttack currentAttack {get; private set;}
        private bool _locked;
        private int _counter;

        [Header("Combo Settings")]
        [SerializeField] private string _graphName;
        [SerializeField] private float chainTime;
        private CountdownTimer _chainTimer;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _chainTimer = new CountdownTimer(chainTime);
            _chainTimer.OnTimerStop += ClearAttackCache;
            _dependencies.TryGetFeature(out attack);
            attack.OnStartAttack += OnStartAttack;
            attack.OnEndAttack += OnEndAttack;
            
            _graph = Resources.Load<ScriptableObject>($"ComboGraph/{_graphName}") as ComboContainer;
        }

        private void OnDisable()
        {
            attack.OnStartAttack -= OnStartAttack;
            attack.OnEndAttack -= OnEndAttack;
        }

        public override void UpdateFeature()
        {
            _chainTimer.Tick(Time.deltaTime);
        }

        public override void Apply(ref InputPayload @event)
        {
            if(_locked) return;

            string signal = @event.Signal;
            if(string.IsNullOrWhiteSpace(signal)) return;
            
            bufferSignal = signal.Split(InputBuffer.Separator).ToList();
            TraverseGraph();
        }

        private void LoadNode(NodeData nodeData)
        {
            if (_graph == null || nodeData == null) return;
            
            _actualCombo = nodeData;

            if (_actualCombo != null)
            {
                _counter = 0;
                _transitions = _graph.links.Where(link => link.sourceNodeGuid == _actualCombo.nodeGuid).ToList();
                LoadAttack();
            } 
        }

        private bool Match(string transition)
        {
            int count = bufferSignal.Count;
            int layers = InputBuffer.Instance.layerCount;
            if (count != layers) return false;
            var temp = transition.Split(InputBuffer.Separator).ToList();
            string all = InputBuffer.AllKeyword;

            for (int i = 0; i < layers && i < temp.Count; i++)
            {
                if(EQ(temp[i],all)) continue;
                if (!EQ(temp[i], bufferSignal[i])) return false;
            }
            return true;
        }

        //Case Unsensitive And Trimmed
        public bool EQ(string lhs, string rhs)
        {
            lhs = lhs.Trim().ToLower();
            rhs = rhs.Trim().ToLower();
            return lhs.Equals(rhs);
        }

        private void TraverseGraph()
        {
            if (_graph == null) return;

            var entry = _graph.nodes.First(node => node.isEntryPoint);
            var entryTransition = _graph.links.Where(link => link.sourceNodeGuid == entry.nodeGuid && link != _transition);
            if (_actualCombo != null)
            {
                entryTransition.Union(_graph.links.Where(link => link.sourceNodeGuid == _actualCombo.nodeGuid));
            }

            foreach (var transition in entryTransition.ToList())
            {
                var tag = transition.tag;
                if (Match(tag))
                {
                    var targetNode = _graph.nodes.First(node => node.nodeGuid == transition.targetNodeGuid);
                    attack.WaitForReleaseOrInterruption();
                    LoadNode(targetNode);
                    _transition = transition;
                    return;
                }
            }

            if (Match(_transition.tag)) LoadAttack();
        }

        private void LoadAttack()
        {
            if (_actualCombo == null || _actualCombo.attacks.Count == 0) return;
            
            var attacks = _actualCombo.attacks;
            if (_counter >= attacks.Count) _counter = 0;
            
            var retrieveAttack = AttackLibrary.GetAttack(attacks[_counter]);

            if (retrieveAttack != null)
            {
                currentAttack = retrieveAttack;
                attack.StartAttack(currentAttack);
            }
        }

        private void ClearAttackCache()
        {
            _actualCombo = null;
            _transitions.Clear();
            currentAttack = null;
            _transition = null;
            _counter = 0;
        }

        private void OnStartAttack()
        {
            _locked = true;
            _counter++;
        }

        private void OnEndAttack()
        {
            _locked = false;
            _chainTimer.Start();
        }
    }
}