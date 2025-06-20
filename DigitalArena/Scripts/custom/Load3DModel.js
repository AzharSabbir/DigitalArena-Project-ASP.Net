import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { RGBELoader } from 'three/addons/loaders/RGBELoader.js';

const canvas = document.getElementById('viewer');
const modelPath = canvas.dataset.modelPath;

const scene = new THREE.Scene();
scene.background = new THREE.Color(0xf0f0f0);

const camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.1, 1000);
camera.position.set(5, 3, 8);

const renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.outputEncoding = THREE.sRGBEncoding;
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;

const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.dampingFactor = 0.05;
controls.enableZoom = true;
controls.minDistance = 0.01;
controls.maxDistance = 50;
controls.enablePan = true;
controls.autoRotate = true;
controls.autoRotateSpeed = 1.0;

// ✅ Use physically correct lighting and filmic tone mapping
renderer.physicallyCorrectLights = true;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.75; // Increased brightness

// ✅ Ambient light — soft global fill
const ambientLight = new THREE.AmbientLight(0xffffff, 0.5); // Brighter than before
scene.add(ambientLight);

// ✅ Key light — strong shadow and highlight source
const keyLight = new THREE.DirectionalLight(0xffffff, 2.5); // Brighter
keyLight.position.set(10, 10, 10);
keyLight.castShadow = true;
keyLight.shadow.mapSize.set(2048, 2048);
keyLight.shadow.camera.near = 0.5;
keyLight.shadow.camera.far = 50;
keyLight.shadow.camera.left = -10;
keyLight.shadow.camera.right = 10;
keyLight.shadow.camera.top = 10;
keyLight.shadow.camera.bottom = -10;
scene.add(keyLight);

// ✅ Fill light — balances shadows
const fillLight = new THREE.DirectionalLight(0xffffff, 1.2); // More intensity
fillLight.position.set(-8, 6, 4);
scene.add(fillLight);

// ✅ Rim light — makes metal/glass pop from background
const rimLight = new THREE.DirectionalLight(0xffffff, 1.0);
rimLight.position.set(0, 8, -10);
scene.add(rimLight);


// Ground
const ground = new THREE.Mesh(
    new THREE.PlaneGeometry(200, 200),
    new THREE.ShadowMaterial({ opacity: 0.15 })
);
ground.rotation.x = -Math.PI / 2;
ground.receiveShadow = true;
scene.add(ground);


// Load GLB model
const loader = new GLTFLoader();
loader.load(modelPath, (gltf) => {
    const model = gltf.scene;

    model.traverse(child => {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;
        }
    });

    scene.add(model);

    // Fit model to view
    const box = new THREE.Box3().setFromObject(model);
    const size = box.getSize(new THREE.Vector3()).length();
    const center = box.getCenter(new THREE.Vector3());

    controls.target.copy(center);
    camera.position.copy(center).add(new THREE.Vector3(size * 1.5, size * 0.7, size * 1.5));
    camera.lookAt(center);
}, undefined, (error) => {
    console.error('Error loading model:', error);
});

function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}

animate();

window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});
