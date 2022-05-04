using UnityEngine;

namespace AwesomeCharts {

    [System.Serializable]
    public class LineEntry : Entry {

        [SerializeField]
        private float position;
        public string Extras;

        public LineEntry() : base() {
            this.position = 0f;
        }

        public LineEntry(float position, float value, string extras) : base(value) {
            this.position = position;
            this.Extras = extras;
        }

        public float Position {
            get { return position; }
            set { position = value; }
        }

        override public float Value {
            get { return value; }
            set { this.value = value; }
        }
    }
}