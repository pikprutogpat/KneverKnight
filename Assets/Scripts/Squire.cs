﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Squire : MonoBehaviour
{
    public CharacterController2D controller;

    public float runSpeed = 40f;
    public bool isCarrying = false, isWalking, isAnimatingMovement;
    public int currentHealth = 100;
    public int startingHealth = 100;
    public int bombs = 2;
    public Slider healthSlider;
    public Text bombAmount;
    public Transform sword;
    public Transform shield;
    public GameObject bomb;

    private float horizontalMove = 0f;
    private bool jump = false;
    private bool isDead = false;
    private Transform top;
    private Animator myAnim;
    LevelManager levelManager;
    Item carriedItem;
    Bomb bombScript;

    private void Awake()
    {
        levelManager = GameObject.FindWithTag("GameController").GetComponent<LevelManager>();
        sword = transform.Find("Graphics").transform.Find("Squire_Body").transform.Find("Squire_ArmRight").transform.Find("Sword");
        shield = transform.Find("Graphics").transform.Find("Squire_Body").transform.Find("Squire_ArmRight").transform.Find("Shield");
        top = transform.Find("Ceilingcheck");
        bombScript = bomb.GetComponent<Bomb>();
        sword.gameObject.SetActive(false);
        shield.gameObject.SetActive(false);
        bombAmount.text = "X " + bombs;
        healthSlider.value = startingHealth;
        myAnim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && isCarrying)
        {
            if (sword.gameObject.activeSelf)
            {
                sword.gameObject.SetActive(false);
            }
            else if (shield.gameObject.activeSelf)
            {
                shield.gameObject.SetActive(false);
            }

            isCarrying = false;
            carriedItem.transform.position = top.transform.position;
            carriedItem.gameObject.SetActive(true);
            carriedItem.Thrown();
        }

        if (Input.GetKeyDown(KeyCode.F) && bombs > 0 && bombScript.throwReady)
        {
            bombs -= 1;
            bombAmount.text = "X " + bombs;
            bomb.transform.position = top.transform.position;
            bomb.gameObject.SetActive(true);
            bombScript.Thrown();
        }

        if (Input.GetButtonDown("Cancel"))
        {
            levelManager.LoadLevel("Menu");
        }
    }
    private void FixedUpdate()
    {
        if (controller.m_Rigidbody2D.velocity.x > 1f || controller.m_Rigidbody2D.velocity.x < -1f && !isWalking)
        {
            if (!isAnimatingMovement)
            {
                isAnimatingMovement = true;
            }
        }
        else
        {
            isWalking = false;
            isAnimatingMovement = false;
        }
        controller.Move(horizontalMove * Time.deltaTime, false, jump);
        jump = false;
        myAnim.SetBool("IsWalking", isAnimatingMovement);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        healthSlider.value = currentHealth;

        if (isCarrying)
        {
            if (sword.gameObject.activeSelf)
            {
                sword.gameObject.SetActive(false);
            }
            else if (shield.gameObject.activeSelf)
            {
                shield.gameObject.SetActive(false);
            }

            isCarrying = false;
            carriedItem.transform.position = top.transform.position;
            carriedItem.gameObject.layer = 13;
            carriedItem.gameObject.SetActive(true);
            carriedItem.Dropped();
        }

        if (currentHealth <= 0 && !isDead)
        {
            Death();
        }
    }

    private void Death()
    {
        isDead = true;
        runSpeed = 0f;
        controller.m_JumpForce = 0f;
        myAnim.SetTrigger("StartDying");
        StartCoroutine(WaitForDeathScene());
    }

    IEnumerator WaitForDeathScene()
    {
        yield return new WaitForSeconds(5);
        levelManager.LoadLevel("GameOver");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 12 && !isCarrying)
        {
            carriedItem = collision.gameObject.GetComponent<Item>();
            collision.gameObject.SetActive(false);
            isCarrying = true;

            if (collision.gameObject.tag == "Sword")
            {
                print("picked up sword");
                sword.gameObject.SetActive(true);
            }
            else if (collision.gameObject.tag == "Shield")
            {
                print("picked up shield");
                shield.gameObject.SetActive(true);
            }
        }
    }
}
