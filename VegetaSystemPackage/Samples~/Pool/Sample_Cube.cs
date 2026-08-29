using UnityEngine;
namespace VegetaSystem
{
    public class Sample_Cube : SinglePoolable
    {
        [SerializeField] float speedRotate;
        private bool isActive;

        public override void Get()
        {
            this.gameObject.SetActive(true);
        }

        public override void Release()
        {
            this.gameObject.SetActive(false);
            this.isActive = false;
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
}
