using System;
using System.Collections;
using System.Collections.Generic;

using Amazon.DynamoDBv2;

using UnityEngine;
using UnityEngine.UI;


public class _PHUI_Carousel : MonoBehaviour
{

    [Header("PHUI Carousel Options")]
    [Tooltip("The carousel scrolling time/speed to move to a new item, per measured in seconds")]
    public float CarouselSpeed = 3f;

    [Header("PHUI Carousel Item Options")]
    [Tooltip("The horizontal and vertical spacing applied to each carousel item (works with the horzontal layout group required on the \"PHUI_Carousel_Scroll_Area\" element)")]
    public int CarouselItemSpacing = 120;
    public int CarouselOpenItemSpacing = 240;

    [Header("PHUI Carousel GameObject References")]

    //Nav buttons
    public GameObject NavButtonRight;
    public GameObject NavButtonLeft;
    private Animator NavButtonRightAnimator;
    private Animator NavButtonLeftAnimator;
    [SerializeField] private float CurrentPosNavButtonLeft = 0;
    [SerializeField] private float CurrentPosNavButtonRight = 0;

    //The scrollarea
    public RectTransform panel; // To hold the ScrollPanel
    public float CurrentPosScrollArea = 0;

    //Sams special spot
    public float tileMin;
    public float tileMax;

    public Selection_Tile lastHighlight;
    [SerializeField] private bool IsCarouselSelected = false;

    public List<Selection_Tile> tiles = new List<Selection_Tile>();

    // float arrays for carousel item positions, calculated (current) and initial (reset)
    [SerializeField] private float[] TempItemPositions;
    [SerializeField] private float[] ResetItemPositions;
    public float[] CurrentItemPositions;

    //total items in carousel
//    [SerializeField]
//    private int TotalCarouselItems = 0;
    [SerializeField]
    private int CurrentCarouselItem;
    [SerializeField]
    private int PreviousCarouselItem;

    [SerializeField]
    private bool WaitForLerp = false;

    [SerializeField]
    private bool WaitForCollapse = false;

    //open and closing timing
    [SerializeField]
    private float OpenTileTiming = 2f;
    [SerializeField]
    private float CloseTileTiming = 2f;

    int startingIndex;

    IEnumerator ChangeTileLerp;


    //the first thing that happens - as the app manager adds carousel items, feed it into our own list 
//    public void AddNewCarouselItem(GameObject newItem)
//    {
////        TotalCarouselItems++;
//        //Debug.Log("TotalCarouselItems: " + TotalCarouselItems);
//        CarouselItemList.Add(newItem);
//    }


    //set up the stuff that doesn't need any carousel items to configure 
    void Start()
    {
        //Caching of animators and tranforms
        NavButtonRightAnimator = NavButtonRight.GetComponent<Animator>();
        NavButtonLeftAnimator = NavButtonLeft.GetComponent<Animator>();
}


    //called from the timeline as we need to wait till prefabs are inst. and items have been child-ed to the carousel scrollarea
    public void InitialiseCarousel()
    {
        var tileCount = tiles.Count;
        
        //once we have items in the carousel we can sort out the starting positions and carousel look
        TempItemPositions    = new float[tileCount];
        ResetItemPositions   = new float[tileCount];
        CurrentItemPositions = new float[tileCount];

        //set the starting tile - the middle one, handles odd and even totals - and account for lists starting at 0     
        //Debug.Log("TotalCarouselItems " + TotalCarouselItems);

        startingIndex       = (int)Math.Ceiling((float)tileCount / 2) - 1;
        CurrentCarouselItem = startingIndex;
        //Debug.Log("CurrentCarouselItem = startingIndex = " + CurrentCarouselItem);

        ShowHideNavButtons();

        Invoke("SetItemStartPositions", 0.1f);
    }


    public void SetItemStartPositions()
    {
        var i = 0;

        foreach (var tile in tiles)
        {

            ResetItemPositions[i]   = i * (tileMin + CarouselItemSpacing);
            CurrentItemPositions[i] = i * (tileMin + CarouselItemSpacing);

            ((RectTransform)tile.transform).anchoredPosition = new Vector2(CurrentItemPositions[i],
                                                                           tile.GetComponent<RectTransform>().anchoredPosition.y);

            //Debug.LogFormat("I'm going through tiles and my index is: {0} | and my new x pos is: {1}", i, CurrentItemPositions[i]);
            i++;
        }

        CurrentPosScrollArea = -CurrentItemPositions[CurrentCarouselItem];

        panel.anchoredPosition = new Vector2(CurrentPosScrollArea,
                                             panel.anchoredPosition.y);


    }


//    public void UpdateItemPositions()
//    {
//        var i = 0;
//        
//        foreach (var tile in tiles)
//        {
//
//            CurrentItemPositions[i] = tile.rectTrans.anchoredPosition.x;
//            
//            //Debug.LogFormat("I'm updating tile pos and my index is: {0} | and my new x pos is: {1}", i, CurrentItemPositions[i]);
//            
//            i++;
//        }
//    }


    //get the sibling index of given carousel item
    public int GetCarouselItemNumber(GameObject carouselItemToFind)
    {
        int index = carouselItemToFind.transform.GetSiblingIndex();
        return index;
    }


    //get the carousel item for a given index
//    public GameObject GetCarouselItemObject(int carouselItemToFind)
//    {
//        var i = 0;
//        
//        GameObject ItemToReturn = null;
//        
//        foreach (var tile in tiles)
//        {
//            if (i == carouselItemToFind)
//            {
//                ItemToReturn = tile.gameObject;
//            }
//            
//            i++;
//        }
//        
//        return ItemToReturn;
//    }

    void MoveTilesWithStateChangeHandler(int itemIndex, bool open)
    {
        if (MovingTiles)
        {
            //Debug.Log("MoveTilesWithStateChangeHandler : Coroutine interupted");
            StopCoroutine(ChangeTileLerp);

        }
        ChangeTileLerp = MoveTilesWithStateChange(itemIndex, open);
        StartCoroutine(ChangeTileLerp);
    }

    bool MovingTiles = false;

    IEnumerator MoveTilesWithStateChange(int itemIndex, bool open) //true = open, false = collapse
    {
        MovingTiles = true;

        //1. get the current x values for the tiles, even if mid animation/lerp  
            var i = 0;
            foreach (var tile in tiles)
            {
                TempItemPositions[i] = tile.rectTrans.anchoredPosition.x;
                i++;
            }

        //2. now onto the new values for the tiles and arrows, based on direction
            var newPosTileItemX = 0f;

            i = 0;
            foreach (var tile in tiles)
            {

                if (open) //open tiles, lerp all except to selected, that one will animate open 
                {
                    //Debug.Log("MoveTilesWithStateChange = open");
                    if (i < itemIndex) // current item is to the left of the selected, move it off to the left with extra spacing
                    {
                        if (ResetItemPositions[i] > 0)
                        {
                            //Debug.Log("1> I'm in the weird consition");
                            CurrentItemPositions[i] = ((-ResetItemPositions[i]) - (CarouselOpenItemSpacing - (tileMax - tileMin)))*-1;
                            //Debug.LogFormat("1> open tile!!! - I'm making new tile positions to the left... my index is: {0} | originally i started at: {1} | i was at: {2} and my new x pos is: {3}", i, ResetItemPositions[i], TempItemPositions[i], CurrentItemPositions[i]);
                        }
                        else
                        {
                            CurrentItemPositions[i] = -(ResetItemPositions[i] - (CarouselOpenItemSpacing - (tileMax - tileMin)));
                            //Debug.LogFormat("2> open tile!!! - I'm making new tile positions to the left... my index is: {0} | originally i started at: {1} | i was at: {2} and my new x pos is: {3}", i, ResetItemPositions[i], TempItemPositions[i], CurrentItemPositions[i]);
                    }

                }
                    else if (i > itemIndex)  // current item is to the right of the selected, move it off to the rigth with extra spacing
                    {
                        CurrentItemPositions[i] = ResetItemPositions[i] - CarouselOpenItemSpacing + (tileMax - tileMin);
                        //Debug.LogFormat("open tile!!! - I'm making new tile positions to the right... my index is: {0} | i was at: {1} and my new x pos is: {2}", i, TempItemPositions[i], CurrentItemPositions[i]);
                    }
                    else
                    {
                        //Debug.LogFormat("Selected tile!!! - ... my index is: {0} | i was at: {1} and my new x pos is: {2}", i, TempItemPositions[i], CurrentItemPositions[i]);
                    }
                }
                else //close tiles, easy - reset them all to their intial values for lerping 
                {
                    //Debug.Log("MoveTilesWithStateChange = closing");
                    CurrentItemPositions[i] = ResetItemPositions[i];
                }
                i++;
            }

            //the current positions of the nav buttons
            CurrentPosNavButtonLeft = NavButtonLeft.GetComponent<RectTransform>().anchoredPosition.x;
            CurrentPosNavButtonRight = NavButtonRight.GetComponent<RectTransform>().anchoredPosition.x;


        //3. do the actual lerping for tiles and arrows with all the calculated values 
            var timing = 0f;
            var newPosNavButtonLeftX = 0f;
            var newPosNavButtonRightX = 0f;



            while (timing < OpenTileTiming)
            {
                timing += Time.deltaTime;

                //Debug.LogFormat("timing {0} | Time.deltaTime: {1} | OpenCollapseTiming {2} | (Time.deltaTime / OpenCollapseTiming) {3}", timing, Time.deltaTime, OpenCollapseTiming, (Time.deltaTime / OpenCollapseTiming));

                //lerp the tiles from the temp values (TempItemPositions) to their new calculated values (CurrentItemPositions)
                i = 0;
                foreach (var tile in tiles)
                {
                    newPosTileItemX = Mathf.Lerp(TempItemPositions[i], CurrentItemPositions[i], (timing / OpenTileTiming)*CarouselSpeed);
                    //Debug.LogFormat("i: {0} | timing: {1} | curX: {2} | newX: {3} for a value of {4}",i,timing, TempItemPositions[i], CurrentItemPositions[i], newPosTileItemX);
                    tile.rectTrans.anchoredPosition = new Vector2(newPosTileItemX, tile.rectTrans.anchoredPosition.y);
                    i++;
                }

                if (open) //open tiles, lerp arrows from current pos to -20/20 
                {
                    newPosNavButtonLeftX = Mathf.SmoothStep(CurrentPosNavButtonLeft, 400f, timing / OpenTileTiming);
                    newPosNavButtonRightX = Mathf.SmoothStep(CurrentPosNavButtonRight, -400f, timing / OpenTileTiming);
                }
                else //close tiles, lerp arrows from current pos to -400/400 
                {
                    newPosNavButtonLeftX = Mathf.SmoothStep(CurrentPosNavButtonLeft, 20f, timing / OpenTileTiming);
                    newPosNavButtonRightX = Mathf.SmoothStep(CurrentPosNavButtonRight, -20f, timing / OpenTileTiming);
                }

                //Debug.LogFormat("AccountForCollapsingTiles : L >> at {0}, going to newLButtonX: {1}", CurrentPosNavButtonLeft, newPosNavButtonLeftX);
                NavButtonLeft.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPosNavButtonLeftX, NavButtonLeft.GetComponent<RectTransform>().anchoredPosition.y);

                //Debug.LogFormat("AccountForCollapsingTiles : R >> at {0}, going to newRButtonX: {1}", CurrentPosNavButtonRight, newPosNavButtonRightX);
                NavButtonRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPosNavButtonRightX, NavButtonRight.GetComponent<RectTransform>().anchoredPosition.y);

                yield return null;

            }

        WaitForCollapse = false;
        MovingTiles = false;
    }


    public void SetCarouselItemIndex(int itemIndex)
    {
        CurrentCarouselItem = itemIndex;            
    }


    IEnumerator MoveCarousel(bool openTile)
    {
        //if (!WaitForLerp)
        //{
        //WaitForLerp = true;
        //close open tiles and adjust positions
        if (WaitForCollapse)
            {
            //start Navbutton FadeinOut animation
                MoveTilesWithStateChangeHandler(PreviousCarouselItem,false);
                //Debug.LogFormat(" === close done... NOW I GONNA LERP!!!! to " + -CurrentItemPositions[CurrentCarouselItem]);
            }

        //lerping to the position of the selected tile, open or not 
        float targetPos = -(CurrentItemPositions[CurrentCarouselItem]);
        //get the current acroll position
        CurrentPosScrollArea = panel.anchoredPosition.x;

        //if (IsCarouselSelected) { targetPos = -(CurrentItemPositions[CurrentCarouselItem]); }
        //else { targetPos = -CurrentItemPositions[startingIndex]; }

        StartCoroutine(LerpToPosition(targetPos));
        //click on tile directly >> we want to open tiles to collapse, lerp to selected tile and then open it 
        if (openTile && this.lastHighlight != null && IsCarouselSelected) 
            {
                //Debug.Log("Starting opening of tile " + CurrentCarouselItem);
                MoveTilesWithStateChangeHandler(CurrentCarouselItem, true);
                this.lastHighlight.GetComponent<Selection_Tile>().SetTileActive();
            }

        //}
        //else
        //{
        //    Debug.LogFormat("can't lerp, waiting for my time");
        //}

        yield return null;
    }


    IEnumerator LerpToPosition(float position)
    {
        //float newX = Mathf.Lerp(panel.anchoredPosition.x, position, Time.deltaTime * CarouselSpeed);

        //Debug.Log("LerpToPosition : CurrentPosScrollArea : " + CurrentPosScrollArea);

        float newX=0;
        //Debug.LogFormat(">> : Posiiton Lerping! : newX: {0}  position: {1} ", newX, position);

        float timing = 0f;

        while (timing < (OpenTileTiming))
        {
            timing += Time.deltaTime;


            newX = Mathf.SmoothStep(CurrentPosScrollArea, position, (timing / OpenTileTiming) * CarouselSpeed);
            //Debug.LogFormat("#1 : Posiiton Lerping! : CurrentPosScrollArea: {0}  position: {1}  newX {2}", CurrentPosScrollArea, position, newX);

            //if (Mathf.Abs(position - newX) < 0.5f)
            //{
            //    Debug.LogFormat("Breaking the lerp");
            //    panel.anchoredPosition = new Vector2(CurrentPosScrollArea, panel.anchoredPosition.y);
            //    //WaitForLerp = false;
            //    break;
            //}

            //Debug.LogFormat("#2 : Posiiton Lerping! : CurrentPosScrollArea: {0}  position: {1}  newX {2}", CurrentPosScrollArea, position, newX);

            panel.anchoredPosition = new Vector2(newX, panel.anchoredPosition.y);

            yield return null;

        }

        //Debug.LogFormat("Out of the lerp loop!");
        CurrentPosScrollArea = position;

        //WaitForLerp = false;
    }


    public void AddNewHighlightTile(Selection_Tile lastHighlight)
    {
        PreviousCarouselItem = CurrentCarouselItem;

        if (this.lastHighlight != null)
        {
            this.lastHighlight.GetComponent<Selection_Tile>().SetTileInactive();
            WaitForCollapse = true;
        }
        this.lastHighlight = lastHighlight;

        SetCarouselItemIndex(GetCarouselItemNumber(lastHighlight.gameObject));
        IsCarouselSelected = true;
        StartCoroutine(MoveCarousel(true));

        ShowHideNavButtons();
    }

    public void MoveCarouselLeft()
    {
        Debug.LogFormat("In MoveCarouselLeft : WaitForLerp: " + WaitForLerp);
        //if (!WaitForLerp)
        //{
            //Debug.LogFormat("In MoveCarouselLeft with {0} > 0", CurrentCarouselItem);
            if (CurrentCarouselItem > 0)
            {
                //Debug.LogFormat("In MoveCarouselLeft - and i'm in");
                PreviousCarouselItem = CurrentCarouselItem;
                if (this.lastHighlight != null)
                {
                    this.lastHighlight.GetComponent<Selection_Tile>().SetTileInactive();
                    WaitForCollapse = true;
                }
                lastHighlight = null;
                IsCarouselSelected = false;
                CurrentCarouselItem--;
                SetCarouselItemIndex(CurrentCarouselItem);
                StartCoroutine(MoveCarousel(false));
            }
            ShowHideNavButtons();
        //}
    }


    public void MoveCarouselRight()
    {
        Debug.LogFormat("In MoveCarouselRight : WaitForLerp: " + WaitForLerp);
        //if (!WaitForLerp)
        //{
            //Debug.LogFormat("In MoveCarouselRight with {0} > {1}", CurrentCarouselItem, TotalCarouselItems - 1);
            if (CurrentCarouselItem < tiles.Count - 1)
            {
                //Debug.LogFormat("In MoveCarouselRight - and i'm in");
                PreviousCarouselItem = CurrentCarouselItem;
                if (lastHighlight != null)
                {
                    lastHighlight.GetComponent<Selection_Tile>().SetTileInactive();
                    WaitForCollapse = true;
                }
                lastHighlight = null;
                IsCarouselSelected = false;
                CurrentCarouselItem++;
                SetCarouselItemIndex(CurrentCarouselItem);
                StartCoroutine(MoveCarousel(false));
            }
            ShowHideNavButtons();
        //}
    }


    void ShowHideNavButtons()
    {
        if (tiles.Count > 1)
        //if (IsCarouselSelected) 
        {
            //Debug.Log("ShowHideNavButtons : CurrentCarouselItem = " + CurrentCarouselItem);
            //show or hide left/right carousel arrows
            if (CurrentCarouselItem == 0)
            {
                //Debug.Log("ShowHideNavButtons : 1ST COND");
                NavButtonLeft.SetActive(false);
                NavButtonRight.SetActive(true);
            }
            else if (CurrentCarouselItem == tiles.Count - 1)
            {
                //Debug.Log("ShowHideNavButtons : 2ndCOND");
                NavButtonLeft.SetActive(true);
                NavButtonRight.SetActive(false);
            }
            else
            {
                //Debug.Log("ShowHideNavButtons : 3rd COND");
                NavButtonLeft.SetActive(true);
                NavButtonRight.SetActive(true);
            }
        }
        else
        {
            //Debug.Log("ShowHideNavButtons : less then 1 tile available");
            NavButtonLeft.SetActive(false);
            NavButtonRight.SetActive(false);
        }

    }

    public void LerpToAndShowSelected()
    {
        StartCoroutine(MoveCarousel(true));
    }
}