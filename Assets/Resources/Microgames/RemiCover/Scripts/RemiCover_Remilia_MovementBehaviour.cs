﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemiCover_Remilia_MovementBehaviour : MonoBehaviour {

    public float walkingSpeed;                      // Speed of Remilia's walking movement.
    public float runningSpeed;                      // Speed of Remilia's running movement.
    private float currentSpeed;                     // Remilia's currentSpeed (Only useful for Running movement)
    public float accelerationFactor;                // How much will CurrentSpeed increase until reaching runningSpeed?
    public float leftLimit, rightLimit;             // Minimum and Maximum value of Remilia's X position that she can take

    // Probabilities for choosing, randomly, different movements for Remilia
    public int walkProbability;                     
    public int standProbability;
    public int runningProbabilty;
    public int teleportProbability;
    private int totalProbabilityValue;

    // Movement actions
    private enum movements { NONE, WALK, STAND, RUN, TELEPORT}      // Movements available
    private int currentMovementSelection = (int) movements.NONE;    // Last movement selected
    private int previousMovementSelection = (int) movements.NONE;   // Previous movement selected
    private float movementTimer = 0;                                // How long will the selected movement be performed? (Will be assigned Randomly).
    public float initialMovementDuration;                           // Duration of the first movement performed by default (STAND movement).
    public float minimumMovementDuration;                           // Minimum value of movement timer can take initially.
    public float maximumMovementDuration;                           // Maximum value of movement timer can take initially.
    public float minTeleportDistance;                               // Minimun distance between point A to point B during teleport movement

    // Movement Direction
    private enum movementDirections { LEFT, RIGHT } // To specify where Remilia is facing (Left by default).
    private int currentMovementDirection = (int) movementDirections.LEFT;    
    private bool facingRight = true;                // To check if checking to the right 


    private Animator anim;
    private RemiCover_Remi_HealthBehaviour healthScript;


    void Start()
    {
        this.totalProbabilityValue = standProbability + walkProbability + runningProbabilty + teleportProbability;
        this.anim = GetComponent<Animator>();
        this.healthScript = GetComponent<RemiCover_Remi_HealthBehaviour>();
        setInitialPosition();
        setInitialMovement( (int) movements.STAND, initialMovementDuration);

    }

    // Update is called once per frame
    void Update()
    {

        if (!MicrogameController.instance.getVictoryDetermined())
        {
            moveCharacter();
        }


    }

    /// <summary>
    // Set the initial position of gameObject according to the position of the cursor
    /// </summary>
    private void setInitialPosition()
    {
        Vector2 mousePosition = CameraHelper.getCursorPosition();
        transform.position = new Vector2(mousePosition.x, this.transform.position.y);
    }


    /// <summary>
    /// Set the initial movement which Remilia will make And the time for which is going to make that movement
    /// </summary>
    private void setInitialMovement(int movement, float timer)
    {
        this.currentMovementSelection = movement;
        this.movementTimer = timer;
        this.previousMovementSelection = (int) movements.NONE;
    }


    /// <summary>
    /// Make Remilia perform certain movement. If no movement is selected to be performed, then choose randomly a movement.
    /// </summary>
    private void moveCharacter()
    {
        if (currentMovementSelection == (int) movements.NONE)
        {
            chooseMovement();
            changeMovementAnimation();
        }

        performMovement();
        
    }


    /// <summary>
    /// Choose a movement for Remilia to perform. Standing movement won't be chosen twice in a row.
    /// </summary>
    private void chooseMovement()
    {

        int rnd_number;
        if (previousMovementSelection == (int) movements.STAND)
        {
            rnd_number = Random.Range(0, totalProbabilityValue - standProbability);         // Standing won't be chosen twice in a row
        }
        else if(previousMovementSelection == (int)movements.TELEPORT)
        {
            rnd_number = Random.Range(0, totalProbabilityValue - teleportProbability - standProbability);       // Teleport won't be chosen twice in a row.
                                                                                                                // Also, if previous movement was teleport stand won't be chosen either.
        }
        else
        {
            rnd_number = Random.Range(0, totalProbabilityValue);
        }


        if (hasWalkingBeenSelected(rnd_number))
        {
            chooseMovementDirection();
            currentMovementSelection = (int) movements.WALK;
        }

        else if (hasRunningBeenSelected(rnd_number))
        {
            chooseMovementDirection();
            currentMovementSelection = (int) movements.RUN;
        }

        else if (hasTeleportBeenSelected(rnd_number))
        {
            currentMovementSelection = (int) movements.TELEPORT;
        }

        else
        {
            currentMovementSelection = (int)movements.STAND;
        }

        movementTimer = Random.Range(minimumMovementDuration, maximumMovementDuration);
    }

    /// <summary>
    /// Make Remilia perform the movement selected currently
    /// </summary>
    
    private void performMovement()
    {


        switch (currentMovementSelection)
        {
            case (int) movements.WALK:
                performWalkMovement();
                break;

            case (int) movements.STAND:
                performStandingMovement();
                break;

            case (int)movements.TELEPORT:
                performTeleportMovement();
                break;

            case (int)movements.RUN:
                performRunMovement();
                break;
        }

        if (currentMovementSelection != (int)movements.TELEPORT) // Teleport is not affected by timer.
        {
            movementTimer = movementTimer - Time.deltaTime;
            if (movementTimer <= 0)
            {
                resetMovementSelectionParameters();
            }
        }

    }


    public void resetMovementSelectionParameters()
    {
        this.previousMovementSelection = this.currentMovementSelection;
        this.currentMovementSelection = (int) movements.NONE;
        this.movementTimer = 0;
    }

    /// <summary>
    /// Check if Walk movement has been selected.
    /// </summary>
    private bool hasWalkingBeenSelected(int number)
    {
        return (number >= 0 && number < walkProbability);
    }

    /// <summary>
    /// Check if Running movement has been selected.
    /// </summary>
    private bool hasRunningBeenSelected(int number)
    {
        return (number >= walkProbability && number < (walkProbability + runningProbabilty));
    }

    /// <summary>
    /// Check if Teleport movement has been selected.
    /// </summary>
    private bool hasTeleportBeenSelected(int number)
    {
        return (number >= (walkProbability + runningProbabilty) && number < (walkProbability + runningProbabilty + teleportProbability));
    }

    /// <summary>
    /// Check if Standing movement has been selected.
    /// </summary>
    private bool hasStandingBeenSelected(int number)
    {
        return (number >= (walkProbability + runningProbabilty + teleportProbability) && number < totalProbabilityValue);
    }

    /// <summary>
    /// Change movement animation
    /// </summary>
    private void changeMovementAnimation()
    {

        switch (previousMovementSelection)
        {

            case (int)movements.RUN:
                if (currentMovementSelection != (int)movements.RUN)
                {
                    anim.SetBool("Run", false);
                }
                break;

            case (int)movements.WALK:
                if (currentMovementSelection != (int)movements.WALK)
                {
                    anim.SetBool("Walk", false);
                }
                break;

            
            case (int)movements.TELEPORT:
                if (currentMovementSelection != (int)movements.TELEPORT)
                {
                    anim.SetBool("Teleport", false);
                }
                break;

        }

        switch (currentMovementSelection)
        {
            case (int)movements.RUN:
                anim.SetBool("Run", true);
                break;

            case (int)movements.WALK:
                anim.SetBool("Walk", true);
                break;

            case (int)movements.TELEPORT:
                anim.SetBool("Teleport", true);
                break;

            case (int)movements.STAND:
                anim.SetBool("Run", false);
                anim.SetBool("Walk", false);
                anim.SetBool("Teleport", false);
                break;
        }
    }


    /// <summary>
    /// Perform Walking Movement
    /// </summary>
    private void performWalkMovement()
    {
        var move = obtainMovementVector3();
        this.transform.position = this.transform.position + (move * this.walkingSpeed * Time.deltaTime);
        this.currentSpeed = walkingSpeed;
        changeDirectionOnLimit();
      
    }

    /// <summary>
    /// Change the position of the character randomly. If the new position is near the previous position, then the new position is moved a little.
    /// </summary>
    private void changePosition()
    {

        float xPosition = transform.position.x;
        float newPosition = xPosition;

        bool optionA = false;
        bool optionB = false;

        if (xPosition - minTeleportDistance > leftLimit )
            optionA = true;
        if (xPosition + minTeleportDistance < rightLimit)
            optionB = true;

        if (optionA && optionB)
        {
            bool chooseOptionA = randomBoolean();
            if (chooseOptionA)
                newPosition = Random.RandomRange(leftLimit, xPosition - minTeleportDistance);
            else
                newPosition = Random.RandomRange(xPosition + minTeleportDistance, rightLimit);
        }
        else if (optionA)
            newPosition = Random.RandomRange(leftLimit, xPosition - minTeleportDistance);
        else
            newPosition = Random.RandomRange(xPosition + minTeleportDistance, rightLimit);
    
        transform.position = new Vector2(newPosition, transform.position.y);
    }


    /// <summary>
    /// Perform Standing Movement
    /// </summary>
    private void performStandingMovement()
    {
        this.currentSpeed = 0;
    }

    /// <summary>
    /// Perform Running movement
    /// </summary>
    private void performRunMovement()
    {
        var move = obtainMovementVector3();
        if (currentSpeed == 0)
        {
            this.currentSpeed = walkingSpeed;
        }
        else if (currentSpeed < runningSpeed)
        {
            this.currentSpeed += accelerationFactor;
        }

        this.transform.position = transform.position + (move * currentSpeed * Time.deltaTime);
        changeDirectionOnLimit();
    }

    /// <summary>
    /// Perform Teleport movement
    /// </summary>
    public void performTeleportMovement()
    {
        // Teleport movement is performed by Animator..-
        if(healthScript.isActiveAndEnabled)
            healthScript.setInmunnity(true);


    }

    /// <summary>
    /// Gets called when teleport movement has ended
    /// </summary>
    public void endTeleportMovement()
    {
        
        anim.SetBool("Teleport", false);

        if (healthScript.isActiveAndEnabled)
            healthScript.setInmunnity(false);

        resetMovementSelectionParameters();
        this.currentSpeed = 0;
        
    }

    /// <summary>
    /// Choose randomly a direction (Left or Right) which Remilia will follow. A new direction won't be chosen if the character was walking or running previously
    /// </summary>
    private void chooseMovementDirection()
    {
        if (previousMovementSelection == (int) movements.WALK || previousMovementSelection == (int) movements.RUN)
        {
            return;
        }

        this.currentMovementDirection = Random.Range(0, 2); // LEFT = 0, RIGHT = 1

        if ((this.currentMovementDirection == (int)movementDirections.RIGHT && facingRight == false) ||
            (this.currentMovementDirection == (int)movementDirections.LEFT && facingRight == true))
        {
            flipHorizontally();
        }
                   
    }


    /// <summary>
    /// Change movement direction if Remi reach the left or right limit.
    /// </summary>
    private void changeDirectionOnLimit()
    {

        if (this.transform.position.x <= leftLimit) this.currentMovementDirection = (int)movementDirections.RIGHT;
        else if (this.transform.position.x >= rightLimit) this.currentMovementDirection = (int)movementDirections.LEFT;

        if (this.currentMovementDirection == (int)movementDirections.RIGHT && facingRight == false) flipHorizontally();
        else if (this.currentMovementDirection == (int)movementDirections.LEFT && facingRight == true) flipHorizontally();
    }


    /// <summary>
    /// Generate a movement vector that depends on the direction of the current movement.
    /// </summary>
    private Vector3 obtainMovementVector3()
    {
        var move = new Vector3(0, 0, 0);
        switch (this.currentMovementDirection)
        {
            case (int)movementDirections.LEFT:
                move = new Vector3(-1, 0, 0);
                break;

            case (int)movementDirections.RIGHT:
                move = new Vector3(1, 0, 0);
                break;
        }
        return move;
    }


    /// <summary>
    /// Flip Remilia's gameobject horizontally.
    /// </summary>
    private void flipHorizontally()
    {
        if (facingRight)
        {
            facingRight = false;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

        else
        {
            facingRight = true;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }


    /// <summary>
    /// Limit Remilia's movement to left and right limit
    /// </summary>
    private void limitMovement()
    {
        this.transform.position = new Vector2(Mathf.Clamp(leftLimit, this.transform.position.x, rightLimit), this.transform.position.y);
    }

    private bool randomBoolean()
    {
        if (Random.value >= 0.5)
        {
            return true;
        }
        return false;
    }
}


