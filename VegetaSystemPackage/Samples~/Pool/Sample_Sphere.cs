using UnityEngine;

namespace VegetaSystem
{
    public class Sample_Sphere : MultiPoolable
    {
        [SerializeField] Sample_SphereType sphereType;
        [SerializeField] float speedRotate;
        private bool isActive = false;

        public override void Get()
        {
            this.gameObject.SetActive(true);
        }

        public override void Release()
        {
            this.gameObject.SetActive(false);
            this.isActive = false;
        }

        public override string GetSubKeyPool()
        {
            return this.sphereType.ToString();
        }

        public void SetActive (bool value)
        {
            this.isActive = value;
        }

        void Update()
        {
            if(!isActive) return;

            transform.Rotate(Vector3.up*speedRotate*Time.deltaTime);
        }
    }

    public enum Sample_SphereType
    {
        Red,
        Blue
    }
}

